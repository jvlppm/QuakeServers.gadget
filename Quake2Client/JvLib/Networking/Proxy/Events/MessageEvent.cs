using System;

namespace Jv.Networking
{
	public class MessageEventArgs : EventArgs
	{
		#region Constructors
		public MessageEventArgs(byte[] data)
		{
			Data = data;
		}
		#endregion

		public byte[] Data { get; set; }
	}

	public delegate void MessageEventHandler(IProxy sender, MessageEventArgs e);
}
