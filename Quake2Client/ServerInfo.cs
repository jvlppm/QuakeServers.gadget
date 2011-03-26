using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quake2;
using Quake2.Client;
using Quake2.Network.Commands.Server;

namespace Quake2Client
{
	class ServerMessage
	{
		public string Type { get; set; }
		public string Message { get; set; }
	}

	[ComVisible(true)]
	public class ServerInfo
	{
		internal bool Updating { get; set; }

		public ServerInfo(string ip)
		{
			_q2Client = new Q2Client();
			_q2Client.UserVars["User"].SetMember("Name", "WindowsGadget");

			_q2Client.OnServerPrint += (s, e) =>
			                           	{
											if(_q2Client.ConnectionStatus != ConnectionStatus.Connected)
												return;

											if (e.Command.Level == Print.PrintLevel.High && e.Command.Message == "ratbot Detect Test ( rbkck &%trf .disconnect )\r\n")
												return;

											if (_lastMessages.Count >= 10)
												_lastMessages.Dequeue();
											_lastMessages.Enqueue(new ServerMessage { Type = e.Command.Level.ToString(), Message = e.Command.Message });
			                           	};

			_q2Client.OnServerUnknownConnectionlessCommand += (s, e) =>
			{
				if (_lastMessages.Count >= 10)
					_lastMessages.Dequeue();
				_lastMessages.Enqueue(new ServerMessage { Type = "Unknown", Message = e.Command });
			};

			_q2Client.OnServerConnectionlessPrint += (s, e) =>
			{
				if (_lastMessages.Count >= 10)
					_lastMessages.Dequeue();
				_lastMessages.Enqueue(new ServerMessage { Type = "Print", Message = e });
			};

			/*_q2Client.OnServerStringPackage += (s, e) =>
			{
				if (_lastMessages.Count >= 10)
					_lastMessages.Dequeue();
				_lastMessages.Enqueue(new ServerMessage { Type = "String", Message = e.Command.Message });
			};*/

			_lastMessages = new Queue<ServerMessage>();
			if (string.IsNullOrEmpty(ip))
				throw new ArgumentException("Ip cannot be null or empty", "ip");
			Ip = ip;
		}

		public string Ip { get; private set; }
		public string Name
		{
			get
			{
				if (Settings == null || !Settings.ContainsKey("hostname"))
					return Ip;
				return Settings["hostname"].ToString();
			}
			set
			{
				Settings = Settings ?? new Dictionary<string, object>();
				if (Settings.ContainsKey("hostname"))
					Settings["hostname"] = value;
				else
					Settings.Add("hostname", value);
			}
		}

		public object GetSetting(string name)
		{
			if (!Settings.ContainsKey(name))
				return string.Empty;
			return Settings[name];
		}

		public string GetPlayers()
		{
			return Json.Extract(Players);
		}

		private readonly Q2Client _q2Client;
		public bool IsConnected
		{
			get { return _q2Client.ConnectionStatus == ConnectionStatus.Connected; }
			set
			{
				if (value)
				{
					if (_q2Client.ConnectionStatus == ConnectionStatus.Disconnected)
						_q2Client.Connect(Ip.Split(':')[0], int.Parse(Ip.Split(':')[1]));
				}
				else
				{
					if (_q2Client.ConnectionStatus == ConnectionStatus.Connected)
					{
						_q2Client.Disconnect();
						_lastMessages.Clear();
					}
				}
			}
		}

		public bool UpdatingConnection
		{
			get
			{
				return _q2Client.ConnectionStatus == ConnectionStatus.Connecting ||
					   _q2Client.ConnectionStatus == ConnectionStatus.Disconnecting;
			}
		}

		public void Say(string message)
		{
			if(_q2Client.ConnectionStatus != ConnectionStatus.Connected)
				return;
			_q2Client.Send("say " + message + "\n");
		}

		internal DateTime LastUpdate { get; set; }

		private readonly Queue<ServerMessage> _lastMessages;

		public string LastMessages { get { return Json.Extract(_lastMessages); } }

		public List<PlayerInfo> Players { get; set; }
		public int NumberOfPlayers { get { return Players == null ? 0 : Players.Where(p => p.Name != "WindowsGadget").Count(); } }

		public Dictionary<string, object> Settings { get; set; }

		public string LastError { get; set; }
	}
}