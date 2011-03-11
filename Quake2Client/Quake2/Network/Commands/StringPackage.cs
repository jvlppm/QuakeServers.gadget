using Jv.Networking;

namespace Quake2.Network.Commands
{
	public interface IStringPackage : ICommand
	{
		string Message { get; set; }
	}

	public class StringPackage : IStringPackage
	{
		public string Message { get; set; }

		//[string message]
		public StringPackage(byte code, ReadRawData data)
		{
			Message = data.ReadString();
			TypeCode = code;
		}
		protected StringPackage(byte code, string message)
		{
			Message = message;
			TypeCode = code;
		}

		#region ICommand Members
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

			data.WriteByte(TypeCode);
			data.WriteString(Message);
		}

		byte TypeCode { get; set; }
		#endregion
	}
}
