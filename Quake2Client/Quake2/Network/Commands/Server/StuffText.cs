using Jv.Networking;

namespace Quake2.Network.Commands.Server
{
	public class StuffText : StringPackage, IServerStringPackage
	{
		//[string message]
		public StuffText(ReadRawData data) : base((byte)ServerCommand.StuffText, data) { }
		public StuffText(string message) : base((byte)ServerCommand.StuffText, message) { }

		public ServerCommand Type { get { return ServerCommand.StuffText; } }
	}
}
