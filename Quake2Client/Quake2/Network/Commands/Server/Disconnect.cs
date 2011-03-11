using Jv.Networking;

namespace Quake2.Network.Commands.Server
{
	public class Disconnect : IServerCommand
	{
		#region ICommand
		public int Size()
		{
			return 1;
		}

		public void WriteTo(WriteRawData data)
		{
			data.WriteByte((byte)Type);
		}

		public ServerCommand Type { get { return ServerCommand.Disconnect; } }
		#endregion
	}
}
