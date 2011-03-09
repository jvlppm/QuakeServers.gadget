using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Quake2Client
{
	/// <summary>
	/// Dados necessários para atualização de gadget
	/// </summary>
	internal interface IGadgetWrapper
	{
		string Href { get; }
	}

	[ComVisible(true)]
	public class Quake2Client : IGadgetWrapper
	{
		internal class AsyncRequest
		{
			public ServerInfo Server { get; set; }
			public UdpClient Socket { get; set; }
		}

		public string Href { get { return "main.html"; } }

		List<ServerInfo> ServerList { get; set; }

		public string Servers { get { return Json.Extract(ServerList); } }
		private readonly byte[] _queryData;

		public Quake2Client()
		{
			var queryData = new List<byte> { 255, 255, 255, 255 };
			queryData.AddRange("status".Select(ch => (byte)ch));
			queryData.Add(10);
			_queryData = queryData.ToArray();

			ServerList = new List<ServerInfo>
			{
				//new ServerInfo("200.226.133.100:27911") { Name = "FRAG #1" },
				//new ServerInfo("200.177.229.248:27912") { Name = "TERRA #1"},
				//new ServerInfo("200.177.229.248:27913") { Name = "TERRA #2"},
				new ServerInfo("jvlppm.no-ip.org:27910") { Name = "Fake"}
			};
		}

		public string GetServerInfo(string ip)
		{
			ServerInfo serverInfo = ServerList.FirstOrDefault(s => s.Ip == ip);

			if(serverInfo == null)
			{
				serverInfo = new ServerInfo(ip);
				ServerList.Add(serverInfo);
			}

			return Json.Extract(serverInfo);
		}

		public void UpdateServers()
		{
			foreach (var server in ServerList)
			{
				string hostname = server.Ip.Split(':')[0];
				int port = int.Parse(server.Ip.Split(':')[1]);

				UdpClient socket = new UdpClient(hostname, port);
				socket.Send(_queryData, _queryData.Length);

				socket.BeginReceive(ReceiveServerInfo, new AsyncRequest { Server = server, Socket = socket });
			}
		}

		private static void ReceiveServerInfo(IAsyncResult ar)
		{
			AsyncRequest req = (AsyncRequest)ar.AsyncState;
			IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 27910);

			byte[] resp = req.Socket.EndReceive(ar, ref serverEP);
			string serverStrResponse = resp.Aggregate(string.Empty, (current, c) => current + (char) c);
			string[] serverResponse = serverStrResponse.Substring(4).Split('\n');

			Dictionary<string, object> serverInfo = new Dictionary<string, object>();
			var serverKeyValues = serverResponse[1].Split('\\');
			for (int i = 1; i < serverKeyValues.Length; i+=2)
				serverInfo.Add(serverKeyValues[i], serverKeyValues[i+1]);

			req.Server.Settings = serverInfo;
			List<PlayerInfo> players = new List<PlayerInfo>();
			for (int i = 2; i < serverResponse.Length; i++)
			{
				string[] pInfo = serverResponse[i].Split(' ');
				if(pInfo.Length == 3)
					players.Add(new PlayerInfo { Name = pInfo[2].Trim('\"'), Frags = pInfo[0], Ping = pInfo[1]});
			}
			req.Server.Players = players;
		}
	}
}
