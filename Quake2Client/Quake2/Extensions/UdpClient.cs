using System.Net;
using System.Net.Sockets;
using Jv.Networking;

namespace Quake2.Extensions
{
	public static class UdpClientExtensions
	{
		private static WriteRawData CreateConnectionlessPackage(string message)
		{
			WriteRawData newPackage = new WriteRawData();
			newPackage.WriteInt(-1);
			newPackage.WriteString(message);
			return newPackage;
		}

		public static void SendConnectionlessString(this UdpClient udp, string message)
		{
			WriteRawData newPackage = CreateConnectionlessPackage(message);
			udp.Send(newPackage.Data.ToArray(), newPackage.CurrentPosition - 1);
		}

		public static void SendConnectionlessString(this UdpClient udp, string format, params object[] args)
		{
			WriteRawData newPackage = CreateConnectionlessPackage(string.Format(format, args));
			udp.Send(newPackage.Data.ToArray(), newPackage.CurrentPosition - 1);
		}

		public static void SendConnectionlessString(this UdpClient udp, IPEndPoint client, string message)
		{
			WriteRawData newPackage = CreateConnectionlessPackage(message);
			udp.Send(newPackage.Data.ToArray(), newPackage.CurrentPosition - 1, client);
		}

		public static void SendConnectionlessString(this UdpClient udp, IPEndPoint client, string format, params object[] args)
		{
			WriteRawData newPackage = CreateConnectionlessPackage(string.Format(format, args));
			udp.Send(newPackage.Data.ToArray(), newPackage.CurrentPosition - 1, client);
		}
	}
}
