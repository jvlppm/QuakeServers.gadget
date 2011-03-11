using System;

namespace Jv.Networking
{
	public class ErrorEventArgs : EventArgs
	{
		#region Constructors
		public ErrorEventArgs(Exception ex)
		{
			Exception = ex;
		}
		#endregion

		public Exception Exception { get; private set; }
	}

	public delegate void ErrorEventHandler(IProxy sender, ErrorEventArgs e);
}
