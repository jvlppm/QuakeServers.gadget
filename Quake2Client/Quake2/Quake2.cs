namespace Quake2
{
	public enum ConnectionStatus
	{
		Disconnected,
		Disconnecting,
		Connecting,
		Connected
	}

	public enum Protocol
	{
		Old = 26,
		Default = 34,
		R1Q2 = 35
	}

	static class Quake2
	{
		public const int MaxClients = 256;
		public const int MaxModels = 256;
		public const int MaxSounds = 256;
		public const int MaxImages = 256;
		public const int MaxLights = 256;
		public const int MaxItems = 256;
		public const int MaxGeneral = MaxClients * 2;

		public static string Version
		{
			get { return "Quake2 .Net v0.1"; }
		}

		public const int MinorProtocolVersionR1Q2 = 1905;
	}
}
