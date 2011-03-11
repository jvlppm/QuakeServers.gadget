using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Jv.Networking;
using Quake2.Client.Events;
using Quake2.Extensions;
using Quake2.Network;
using Quake2.Network.Commands;
using Quake2.Network.Commands.Client;
using Quake2.Network.Commands.Server;
using Quake2.Variables;

namespace Quake2.Client
{
	public class ServerConnection : IDisposable
	{
		#region Events

		public delegate void PackageHandler(ServerConnection sender, Package<IServerCommand> package);
		public event PackageHandler OnPackage;

		public delegate void StringHandler(ServerConnection sender, string text);
		public event StringHandler OnServerConnectionlessPrint;

		public event ConnectionlessCommandEventHandler OnServerUnknownConnectionlessCommand;

		#endregion

		public Q2Client Q2Client { get; private set; }

		public ConnectionStatus Status { get; private set; }

		IPEndPoint ServerIP;
		UdpClient UdpConnection;
		Netchan<IClientCommand> Channel { get; set; }

		public string Host { get; private set; }
		public int Port { get; private set; }

		public string Challenge { get; private set; }

		Timer FlushSendBufferTimer { get; set; }

		public ServerConnection(Q2Client q2Client, string host, int port, int localPort)
		{
			Q2Client = q2Client;
			Q2Client.OnServerDisconnect += Disconnected;

			Host = host;
			Port = port;

			CreateConnection();

			ServerIP = new IPEndPoint(IPAddress.Any, localPort);

			FlushSendBufferTimer = new Timer((int)Q2Client.UserVars["Net"].GetMember("FlushSendBufferTime"))
			                       	{
			                       		AutoReset = true
			                       	};
			((Variable)Q2Client.UserVars["Net"].GetMember("FlushSendBufferTime")).OnValueChanged += UpdateFlushTime;
			FlushSendBufferTimer.Elapsed += delegate
			{
				FlushSendBuffer();
				FlushSendBufferTimer.Enabled = true;
			};
		}

		private void CreateConnection()
		{
			UdpConnection = new UdpClient();
			UdpConnection.Connect(Host, Port);
		}

		void UpdateFlushTime(object sender, ValueChangedEventArgs<object> e)
		{
			FlushSendBufferTimer.Interval = (double)e.NewValue;
		}

		#region Connection
		public void Connect()
		{
			bool startReceive = false;
			if (Status == ConnectionStatus.Disconnected)
				startReceive = true;

			Status = ConnectionStatus.Connecting;
			SendConnectionlessString("getchallenge");

			if (startReceive)
				UdpConnection.BeginReceive(ReceivePacket, this);
		}

		public void Disconnect()
		{
			if (Status == ConnectionStatus.Connected)
			{
				Status = ConnectionStatus.Disconnecting;
				Send(new StringCmd("disconnect"), true);
			}
		}

		private void Disconnected(object sender, EventArgs e)
		{
			FlushSendBufferTimer.Enabled = false;
			Status = ConnectionStatus.Disconnected;
		}
		#endregion

		#region Receive
		static void ReceivePacket(IAsyncResult ar)
		{
			ServerConnection connection = (ServerConnection)ar.AsyncState;
			var udp = connection.UdpConnection;

			if (udp == null)
				return;

			try
			{
				ReadRawData RawData = new ReadRawData(udp.EndReceive(ar, ref connection.ServerIP));

				int sequence = RawData.ReadInt();

				if (sequence == -1)
				{
					string cmd = RawData.ReadString(' ', '\n');
					switch (cmd)
					{
						case "challenge":
							if (connection.Status != ConnectionStatus.Connecting)
								return;

							byte qPort = (byte)(DateTime.Now.Millisecond * 0xff);

							connection.Challenge = RawData.ReadString(' ');
							string serverProtocol = RawData.ReadString(' ');

							Protocol selectedProtocol = Protocol.Default;

							if (serverProtocol.Contains(((int)Protocol.R1Q2).ToString()))
								selectedProtocol = Protocol.R1Q2;

							connection.Channel = new Netchan<IClientCommand>(connection.ServerIP, selectedProtocol, qPort);

							connection.SendConnectionlessString("connect {0} {1} {2} \"{3}\" 1390 1904", (int)connection.Channel.Protocol, qPort, connection.Challenge, connection.Q2Client.UserInfo.Message);
							break;
						case "client_connect":
							if (connection.Status != ConnectionStatus.Connecting)
								return;

							connection.Status = ConnectionStatus.Connected;
							connection.FlushSendBufferTimer.Enabled = true;
							connection.Send(new StringCmd("new"), true);
							break;
						case "print":
							if (connection.OnServerConnectionlessPrint != null)
								connection.OnServerConnectionlessPrint(connection, RawData.ReadString());
							break;

						default:
							if (connection.OnServerUnknownConnectionlessCommand != null)
								connection.OnServerUnknownConnectionlessCommand(connection, new ConnectionlessCommandEventArgs(connection.ServerIP, cmd, RawData));
							break;
					}
				}
				else if (connection.Status == ConnectionStatus.Connected || connection.Status == ConnectionStatus.Disconnecting)
				{
					int sequence_ack = RawData.ReadInt();

					if (connection.Channel.Accept((uint)sequence, (uint)sequence_ack))
					{
						if (connection.OnPackage != null)
							connection.OnPackage(connection, RawData.ReadServerPackage());
					}
				}

				if (connection.Status != ConnectionStatus.Disconnected)
					udp.BeginReceive(ReceivePacket, ar.AsyncState);
			}
			catch (SocketException) { connection.Disconnected(connection, EventArgs.Empty); }
			catch (ObjectDisposedException)
			{
				connection.Disconnected(connection, EventArgs.Empty);
				connection.CreateConnection();
			}
		}
		#endregion

		#region Send
		void FlushSendBuffer()
		{
			if (Status != ConnectionStatus.Connected && Status != ConnectionStatus.Disconnecting)
				return;

			if (Q2Client.UserInfoModified)
			{
				Send(Q2Client.UserInfo, true);
				Q2Client.UserInfoModified = false;
			}

			do
			{
				WriteRawData pkt = Channel.GetNextPacket();

				if (pkt != null)
				{
					for (int i = 0; i <= (int)((Variable)Q2Client.UserVars["Net"].GetMember("PacketDup")).Value; i++)
						UdpConnection.Send(pkt.Data.ToArray(), pkt.Data.Count);
				}
			} while (Channel.PendingReliable && Status != ConnectionStatus.Disconnected);
		}

		void SendConnectionlessString(string message, params object[] args)
		{
			if (UdpConnection != null)
			{
				if (args == null || args.Length == 0)
					UdpConnection.SendConnectionlessString(message);
				else
					UdpConnection.SendConnectionlessString(message, args);
			}
		}

		public void Send(IClientCommand package, bool reliable)
		{
			Channel.Send(package, reliable);
		}
		#endregion

		public void Dispose()
		{
			((Variable)Q2Client.UserVars["Net"].GetMember("FlushSendBufferTime")).OnValueChanged -= UpdateFlushTime;
			FlushSendBufferTimer.Dispose();
			UdpConnection.Close();
		}
	}
}
