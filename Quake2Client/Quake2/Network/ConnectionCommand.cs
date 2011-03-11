using System;
using Quake2.Network.Commands;

namespace Quake2.Network
{
	public class ConnectionCommandEventArgs<CommandType> : EventArgs where CommandType : ICommand
	{
		public ConnectionCommandEventArgs(CommandType command)
		{
			Command = command;
		}

		public CommandType Command { get; private set; }
	}

	public class PreviewConnectionCommandEventArgs<CommandType> : ConnectionCommandEventArgs<CommandType>, IAbortable where CommandType : ICommand
	{
		public PreviewConnectionCommandEventArgs(CommandType command) : base(command) { }
		public bool Abort { get; set; }
	}

	public delegate void CommandEventHandler<CommandType>(object sender, ConnectionCommandEventArgs<CommandType> e) where CommandType : ICommand;
	public delegate void PreviewCommandEventHandler<CommandType>(object sender, PreviewConnectionCommandEventArgs<CommandType> e) where CommandType : ICommand;
}
