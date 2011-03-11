using Jv.Networking;

namespace Quake2.Network.Commands.Client
{
	public class Nop : IClientCommand
	{
		public ClientCommand Type { get { return ClientCommand.Nop; } }

		#region ICommand
		public int Size()
		{
			return 1;
		}

		public void WriteTo(WriteRawData package)
		{
			package.WriteByte((byte)Type);
		}
		#endregion
	}
}
