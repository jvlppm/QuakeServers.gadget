namespace Jv.Networking
{
	public interface IProxy
	{
		void ForwardLocalConnections(int localPort, ConnectionPoint destination);
		void ForwardConnections(ConnectionPoint source, ConnectionPoint destination);

		void SendToSource(byte[] data);
		void SendToDestination(byte[] data);
	}
}
