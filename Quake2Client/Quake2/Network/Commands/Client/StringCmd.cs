using Jv.Networking;

namespace Quake2.Network.Commands.Client
{
	public class StringCmd : StringPackage, IClientStringPackage
	{
		public ClientCommand Type { get { return ClientCommand.StringCmd; } }

		//[string message]
		public StringCmd(ReadRawData data) : base((byte)ClientCommand.StringCmd, data) { }
		public StringCmd(string message) : base((byte)ClientCommand.StringCmd, message) { }
	}
}
