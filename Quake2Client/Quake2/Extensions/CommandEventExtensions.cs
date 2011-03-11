using Quake2.Network;
using Quake2.Network.Commands;

namespace Quake2.Extensions
{
	public static class CommandEventExtensions
	{
		public static bool Check<CommandType>(this PreviewCommandEventHandler<CommandType> eventHandler, object sender, CommandType package) where CommandType : ICommand
		{
			var args = new PreviewConnectionCommandEventArgs<CommandType>(package);
			try
			{
				if (eventHandler != null)
					eventHandler(sender, args);
			}
			catch { }

			return !args.Abort;
		}

		public static void Fire<CommandType>(this CommandEventHandler<CommandType> eventHandler, object sender, CommandType package) where CommandType : ICommand
		{
			try
			{
				if (eventHandler != null)
					eventHandler(sender, new ConnectionCommandEventArgs<CommandType>(package));
			}
			catch { }
		}
	}
}
