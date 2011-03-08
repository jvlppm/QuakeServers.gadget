using System;
using System.Collections.Generic;

namespace Quake2Client
{
	internal class ServerInfo
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
				return Settings["hostname"];
			}
			set
			{
				Settings = Settings ?? new Dictionary<string, string>();
				if (Settings.ContainsKey("hostname"))
					Settings["hostname"] = value;
				else
					Settings.Add("hostname", value);
			}
		}
		public List<PlayerInfo> Players { get; set; }
		public int NumberOfPlayers { get { return Players == null ? 0 : Players.Count; } }

		public Dictionary<string, string> Settings { get; set; }
	}
}