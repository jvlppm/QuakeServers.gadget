using Jv.Networking;
using Quake2.Variables;

namespace Quake2.Network.Commands.Client
{
	public class UserInfo : SerializableVars, IClientStringPackage
	{
		public ClientCommand Type { get { return ClientCommand.UserInfo; } }

		public UserInfo() { }
		public UserInfo(string serializedVars) : base(serializedVars) { }
		//[string serialized_vars]
		public UserInfo(ReadRawData data) : base(data.ReadString().TrimEnd('\n').Trim('\"')) { }

		public string Message
		{
			get { return SerializedVars; }
			set { SerializedVars = value; }
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
