using Jv.Networking;

namespace Quake2.Network.Commands.Client
{
	public class Move : IClientCommand
	{
		public ClientCommand Type { get { return ClientCommand.Move; } }

		public Move(Protocol protocol, ReadRawData data)
		{
			throw new System.NotImplementedException();//identificar cl_protocol
		}

		public int Size()
		{
			throw new System.NotImplementedException();
		}

		public void WriteTo(WriteRawData package)
		{
			throw new System.NotImplementedException();
		}
	}
}
