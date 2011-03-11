using Jv.Networking;

namespace Quake2.Network.Commands.Server
{
	public class CenterPrint : IServerCommand, IServerStringPackage
	{
		public ServerCommand Type { get { return ServerCommand.CenterPrint; } }

		public string Message { get; set; }

		//[string message]
		public CenterPrint(ReadRawData data)
		{
			Message = data.ReadString();
		}
		public CenterPrint(string message)
		{
			Message = message;
		}

		#region ICommand
		public int Size()
		{
			if (string.IsNullOrEmpty(Message))
				return 0;
			return Message.Length + 2;
		}

		public void WriteTo(WriteRawData data)
		{
			if (string.IsNullOrEmpty(Message))
				return;

			data.WriteByte((byte)Type);
			data.WriteString(Message);
		}
		#endregion
	}
}
