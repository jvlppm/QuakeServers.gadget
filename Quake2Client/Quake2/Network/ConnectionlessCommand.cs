using System;
using Jv.Networking;
using System.Net;

namespace Quake2.Network
{
	public class ConnectionlessCommandEventArgs : EventArgs, IHandleable
	{
		public ConnectionlessCommandEventArgs(IPEndPoint senderIP, string command, ReadRawData RawData)
		{
			SenderIP = senderIP;
			Command = command;
			Package = RawData;
		}

		public IPEndPoint SenderIP { get; private set; }
		public string Command { get; private set; }
		public ReadRawData Package { get; private set; }

		public bool Handled { get; set; }
	}

	public delegate void ConnectionlessCommandEventHandler(object sender, ConnectionlessCommandEventArgs args);
}
