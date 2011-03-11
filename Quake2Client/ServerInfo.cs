using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Quake2Client
{
	[ComVisible(true)]
	public class ServerInfo
	{
		public ServerInfo(string ip)
		{
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

		public bool IsConnected { get; set; }

		public List<PlayerInfo> Players { get; set; }
		public int NumberOfPlayers { get { return Players == null ? 0 : Players.Count; } }

		public Dictionary<string, object> Settings { get; set; }
	}
}