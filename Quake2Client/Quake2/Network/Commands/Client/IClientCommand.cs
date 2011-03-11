namespace Quake2.Network.Commands.Client
{
	public interface IClientCommand : ICommand
	{
		ClientCommand Type { get; }
	}

	public interface IClientStringPackage : IClientCommand, IStringPackage { }

	public enum ClientCommand : byte
	{
		Bad = 0x00,
		Nop = 0x01,

		Move = 0x02,				// [[usercmd_t]
		UserInfo = 0x03,			// [string userinfo]
		StringCmd = 0x04,			// [string message]
		Setting = 0x05,				// [short setting, short value] R1Q2 settings support.
		MultiMoves = 0x06
	}
}