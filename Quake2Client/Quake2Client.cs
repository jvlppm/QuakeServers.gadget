using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Jv.Threading;
using Timer = System.Timers.Timer;

namespace Quake2Client
{
	[ComVisible(true)]
	public class Quake2Client : IDisposable
	{
		#region Nested Members
		class AsyncRequest
		{
			public ServerInfo Server { get; set; }
			public UdpClient Socket { get; set; }
		}
		#endregion

		#region Servers
		public List<ServerInfo> StaticServerList { get; private set; }
		public List<ServerInfo> QueryServerList { get; private set; }
		public List<ServerInfo> MasterQ2Servers { get; private set; }
		#endregion

		private readonly List<List<ServerInfo>> _allServersLists;
		private IEnumerable<ServerInfo> AllServers
		{
			get
			{
				return _allServersLists.SelectMany(list => list);
			}
		}

		public string Servers
		{
			get
			{
				return Json.Extract(
					from s in AllServers
					where DateTime.Now.Subtract(s.LastUpdate).TotalSeconds <= 60
					select s.Ip);
			}
		}

		private readonly byte[] _queryData;

		public Quake2Client()
		{
			var queryData = new List<byte> { 255, 255, 255, 255 };
			queryData.AddRange("status\n".Select(ch => (byte)ch));
			_queryData = queryData.ToArray();

			StaticServerList = new List<ServerInfo>
			{
				new ServerInfo("200.226.133.100:27911") { Name = "FRAG #1" },
				new ServerInfo("200.177.229.248:27912") { Name = "TERRA #1"},
				new ServerInfo("200.177.229.248:27913") { Name = "TERRA #2"},
			};
			QueryServerList = new List<ServerInfo>();
			MasterQ2Servers = new List<ServerInfo>();

			_allServersLists = new List<List<ServerInfo>>
			{
				StaticServerList,
				QueryServerList,
				MasterQ2Servers,
			};

			Timer masterQ2Servers = new Timer(10 * 1000);
			masterQ2Servers.Elapsed += MasterQ2ServersElapsed;
			masterQ2Servers.Enabled = true;
			MasterQ2ServersElapsed(null, null);
		}

		void MasterQ2ServersElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			WebClient client = new WebClient();
			string servers = client.DownloadString("http://q2servers.com/?mod=action&c=br&raw=0&inc=1");

			foreach (var server in servers.Split('\n'))
			{
				if (string.IsNullOrEmpty(server))
					continue;
				if (AllServers.All(oldServer => oldServer.Ip != server))
					MasterQ2Servers.Add(new ServerInfo(server));
			}
		}

		#region Servers
		public ServerInfo GetServerInfo(string ip)
		{
			ServerInfo serverInfo = AllServers.FirstOrDefault(s => s.Ip == ip);

			if (serverInfo == null)
			{
				serverInfo = new ServerInfo(ip);
				QueryServerList.Add(serverInfo);
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

			foreach (var s in AllServers.Where(s => !s.Updating))
			{
				s.Updating = true;
				string hostname = s.Ip.Split(':')[0];
				int port = int.Parse(s.Ip.Split(':')[1]);
				var sock = new UdpClient(hostname, port);

				var connectionInfo = new { Server = s, Socket = sock };

				var timeoutChecker = Parallel.Start(connectionInfo, info =>
				{
					try
					{
						Thread.Sleep(1000);
						info.Socket.Close();
					}
					catch (ThreadAbortException) { }
				});

				Parallel.Start(connectionInfo, info =>
				{
					try
					{
						info.Socket.Send(_queryData, _queryData.Length);
						var serverEP = new IPEndPoint(IPAddress.Any, 27910);

						var resp = info.Socket.Receive(ref serverEP);
						string serverStrResponse = resp.Aggregate(string.Empty, (current, c) => current + (char)c);
						string[] serverResponse = serverStrResponse.Substring(4).Split('\n');

						var serverInfo = new Dictionary<string, object>();
						var serverKeyValues = serverResponse[1].Split('\\');
						for (int i = 1; i < serverKeyValues.Length; i += 2)
							serverInfo.Add(serverKeyValues[i], serverKeyValues[i + 1]);

						info.Server.Settings = serverInfo;
						var players = new List<PlayerInfo>();
						for (int i = 2; i < serverResponse.Length; i++)
						{
							string[] pInfo = serverResponse[i].Split(' ');
							if (pInfo.Length >= 3)
								players.Add(new PlayerInfo
								{
									Name = string.Join(" ", pInfo, 2, pInfo.Length - 2).Trim('\"'),
									Frags = pInfo[0],
									Ping = pInfo[1]
								});
						}
						info.Server.Players = players;
						info.Server.LastError = null;
						info.Server.LastUpdate = DateTime.Now;
						timeoutChecker.Abort(new Exception("Server responded"));
					}
					catch (Exception ex)
					{
						info.Server.LastError = ex.Message;
					}
					finally
					{
						info.Server.Updating = false;
					}
				});
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

		public string CustomArgs
		{
			get { return Settings.ReadValue(RootPath, "Game", "CustomArgs"); }
			set { Settings.WriteValue(RootPath, "Game", "CustomArgs", value); }
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
												 (!string.IsNullOrEmpty(GameCFG) ? " +exec " + GameCFG : "") + " " + CustomArgs +
												 " +connect " + serverIp)
								{
									WorkingDirectory = Path.GetDirectoryName(GamePath)
								};

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

		public void Dispose()
		{
			foreach (var serverInfo in AllServers)
				serverInfo.IsConnected = false;
		}
	}
}
