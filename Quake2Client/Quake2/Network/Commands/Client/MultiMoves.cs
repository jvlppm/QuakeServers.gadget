using Jv.Networking;

namespace Quake2.Network.Commands.Client
{
	public class MultiMoves : IClientCommand
	{
		public ClientCommand Type { get { return ClientCommand.MultiMoves; } }

		public MultiMoves(ReadRawData data)
		{
			throw new System.NotImplementedException();
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
