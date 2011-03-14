using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quake2Client
{
	[ComVisible(true)]
	public class Quake2Client
	{
		#region Nested Members
		class AsyncRequest
		{
			public ServerInfo Server { get; set; }
			public UdpClient Socket { get; set; }
		}
		#endregion

		List<ServerInfo> ServerList { get; set; }

		public string Servers { get { return Json.Extract(ServerList); } }
		private readonly byte[] _queryData;

		public Quake2Client()
		{
			var queryData = new List<byte> { 255, 255, 255, 255 };
			queryData.AddRange("status\n".Select(ch => (byte)ch));
			_queryData = queryData.ToArray();

			ServerList = new List<ServerInfo>
			{
				new ServerInfo("200.226.133.100:27911") { Name = "FRAG #1" },
				new ServerInfo("200.177.229.248:27912") { Name = "TERRA #1"},
				new ServerInfo("200.177.229.248:27913") { Name = "TERRA #2"},
				//new ServerInfo("jvlppm.no-ip.org:27910") { Name = "Fake"}
			};
		}

		#region Servers
		public ServerInfo GetServerInfo(string ip)
		{
			ServerInfo serverInfo = ServerList.FirstOrDefault(s => s.Ip == ip);

			if (serverInfo == null)
			{
				serverInfo = new ServerInfo(ip);
				ServerList.Add(serverInfo);
			}

			return serverInfo;
		}

		public void UpdateServers()
		{
			if (Process.GetProcesses().Any(p => p.ProcessName.ToLower().Contains("q2")))
			{
				_lastPlay = DateTime.Now;
				_externalPlaying = true;
			}
			else _externalPlaying = false;

			foreach (var server in ServerList)
			{
				try
				{
					string hostname = server.Ip.Split(':')[0];
					int port = int.Parse(server.Ip.Split(':')[1]);

					var socket = new UdpClient(hostname, port);
					socket.Send(_queryData, _queryData.Length);

					socket.BeginReceive(ReceiveServerInfo, new AsyncRequest { Server = server, Socket = socket });
				}
				catch (Exception ex)
				{
					server.LastError = ex.Message;
				}
			}
		}

		private static void ReceiveServerInfo(IAsyncResult ar)
		{
			var req = (AsyncRequest)ar.AsyncState;
			var serverEP = new IPEndPoint(IPAddress.Any, 27910);

			try
			{
				byte[] resp = req.Socket.EndReceive(ar, ref serverEP);
				string serverStrResponse = resp.Aggregate(string.Empty, (current, c) => current + (char)c);
				string[] serverResponse = serverStrResponse.Substring(4).Split('\n');

				var serverInfo = new Dictionary<string, object>();
				var serverKeyValues = serverResponse[1].Split('\\');
				for (int i = 1; i < serverKeyValues.Length; i += 2)
					serverInfo.Add(serverKeyValues[i], serverKeyValues[i + 1]);

				req.Server.Settings = serverInfo;
				var players = new List<PlayerInfo>();
				for (int i = 2; i < serverResponse.Length; i++)
				{
					string[] pInfo = serverResponse[i].Split(' ');
					if (pInfo.Length == 3)
						players.Add(new PlayerInfo { Name = pInfo[2].Trim('\"'), Frags = pInfo[0], Ping = pInfo[1] });
				}
				req.Server.Players = players;
				req.Server.LastError = null;
			}
			catch (Exception ex)
			{
				req.Server.LastError = ex.Message;
			}
		}
		#endregion

		#region Settings

		public string RootPath { get; set; }

		private DateTime _lastPlay = DateTime.MinValue;
		public int MinutesSinceLastPlay
		{
			get { return (int)DateTime.Now.Subtract(_lastPlay).TotalMinutes; }
		}

		private bool _internalPlaying, _externalPlaying;
		public bool IsPlaying { get { return _internalPlaying || _externalPlaying; } }

		#region Game
		public string GamePath
		{
			get { return Settings.ReadValue(RootPath, "Game", "Path"); }
			private set { Settings.WriteValue(RootPath, "Game", "Path", value); }
		}
		public string GameCFG
		{
			get { return Settings.ReadValue(RootPath, "Game", "CFG"); }
			private set { Settings.WriteValue(RootPath, "Game", "CFG", value); }
		}

		private string _latchedGamePath;
		public string LatchedGamePath
		{
			get { return _latchedGamePath ?? GamePath; }
			set { _latchedGamePath = value; }
		}

		private string _latchedGameCFG;
		public string LatchedGameCFG
		{
			get { return _latchedGameCFG ?? GameCFG; }
			set { _latchedGameCFG = value; }
		}

		public void BrowseGamePath()
		{
			var dialog = new OpenFileDialog
							{
								Filter = "Arquivos Executáveis|*.exe",
								Title = "Selecione o executável do jogo"
							};
			if (!string.IsNullOrEmpty(LatchedGamePath))
				dialog.InitialDirectory = Path.GetDirectoryName(LatchedGamePath);

			if (dialog.ShowDialog() == DialogResult.OK)
				LatchedGamePath = dialog.FileName;
		}

		public void BrowseGameCFG()
		{
			if (string.IsNullOrEmpty(LatchedGamePath))
				throw new Exception("GamePath must be set");

			var dialog = new OpenFileDialog
			{
				Filter = "Arquivos Config|*cfg",
				Title = "Selecione o arquivo config",
				InitialDirectory = Path.GetDirectoryName(LatchedGamePath) + "\\Action"
			};
			if (dialog.ShowDialog() == DialogResult.OK)
				LatchedGameCFG = Path.GetFileName(dialog.FileName);
		}

		public void SaveSettings()
		{
			GamePath = LatchedGamePath;
			GameCFG = LatchedGameCFG;
			LatchedGamePath = null;
			LatchedGameCFG = null;
		}

		public void DiscardSettings()
		{
			LatchedGamePath = null;
			LatchedGameCFG = null;
		}
		#endregion

		#region Gadget

		public bool AutoLaunch
		{
			get
			{
				bool result;
				return bool.TryParse(Settings.ReadValue(RootPath, "Gadget", "AutoLaunch"), out result) ? result : false;
			}
			set { Settings.WriteValue(RootPath, "Gadget", "AutoLaunch", value.ToString()); }
		}

		public int AutoLaunchMinPlayers
		{
			get
			{
				int result;
				return int.TryParse(Settings.ReadValue(RootPath, "Gadget", "AutoLaunchMinPlayers"), out result) ? result : 1;
			}
			set { Settings.WriteValue(RootPath, "Gadget", "AutoLaunchMinPlayers", value.ToString()); }
		}

		public int AutoLaunchMinTime
		{
			get
			{
				int result;
				return int.TryParse(Settings.ReadValue(RootPath, "Gadget", "AutoLaunchMinTime"), out result) ? result : 60;
			}
			set { Settings.WriteValue(RootPath, "Gadget", "AutoLaunchMinTime", value.ToString()); }
		}

		#endregion

		#endregion

		public void LaunchGame(string serverIp)
		{
			if (string.IsNullOrEmpty(GamePath))
				throw new Exception("GamePath must be set");

			var startInfo = new ProcessStartInfo(GamePath,
												 "+set game action " +
												 (!string.IsNullOrEmpty(GameCFG) ? " +exec " + GameCFG : "") + " +connect " +
												 serverIp);

			var q2 = new Process
			{
				EnableRaisingEvents = true,
				StartInfo = startInfo
			};

			_internalPlaying = true;
			_lastPlay = DateTime.Now;

			q2.Exited += delegate { _internalPlaying = false; };

			q2.Start();
		}
	}
}
