using System;

namespace Quake2.Client.Events
{
	public class ExecuteMethodEventArgs : EventArgs, IAbortable
	{
		public string OriginalCommand { get; private set; }
		public string FinalCommand { get; private set; }

		public bool Abort { get; set; }

		public ExecuteMethodEventArgs(string originalCommand, string finalCommand)
		{
			OriginalCommand = originalCommand;
			FinalCommand = finalCommand;
		}
	}

	public delegate void ExecuteMethodEventHandler(object sender, ExecuteMethodEventArgs e);
}
