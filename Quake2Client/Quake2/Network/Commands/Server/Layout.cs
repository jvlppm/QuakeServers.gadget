using Jv.Networking;

namespace Quake2.Network.Commands.Server
{
	public class Layout : IServerCommand, IServerStringPackage
	{
		public string Message { get; set; }

		//[string message]
		public Layout(ReadRawData data)
		{
			Message = data.ReadString();
		}
		public Layout(string message)
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

		public ServerCommand Type { get { return ServerCommand.Layout; } }
		#endregion
	}
}
