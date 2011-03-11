using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quake2;
using Quake2.Client;

namespace Quake2Client
{
	[ComVisible(true)]
	public class ServerInfo
	{
		public ServerInfo(string ip)
		{
			_q2Client = new Q2Client();

			_q2Client.OnServerPrint += (s, e) =>
			                           	{
											if (_lastMessages.Count >= 10)
												_lastMessages.Dequeue();
											_lastMessages.Enqueue(e.Command.Message);
			                           	};

			_lastMessages = new Queue<string>();
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

		private readonly Queue<string> _lastMessages;

		public string LastMessages { get { return Json.Extract(_lastMessages); } }

		public List<PlayerInfo> Players { get; set; }
		public int NumberOfPlayers { get { return Players == null ? 0 : Players.Count; } }

		public Dictionary<string, object> Settings { get; set; }
	}
}