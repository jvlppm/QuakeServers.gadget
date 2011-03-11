using System;
using System.Net;

namespace Jv.Networking
{
	public class ConnectionPoint
	{
		public ConnectionPoint(string ip, int port, bool openConnection)
		{
			IPAddress ipAddress = IPAddress.Parse(ip);

			if (ipAddress == null)
				throw new Exception("Parse IP error");

			EndPoint = new IPEndPoint(ipAddress, port);
			OpenConnection = openConnection;
		}

		public ConnectionPoint(IPEndPoint ipEndPoint, bool openConnection)
		{
			EndPoint = ipEndPoint;
			OpenConnection = openConnection;
		}

		public string Ip { get { return EndPoint.Address.ToString(); } }
		public int Port { get { return EndPoint.Port; } }

		public IPEndPoint EndPoint;
		public bool OpenConnection { get; private set; }
	}
}
