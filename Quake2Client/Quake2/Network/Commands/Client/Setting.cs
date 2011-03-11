using Jv.Networking;

namespace Quake2.Network.Commands.Client
{
	public enum SettingType : short
	{
		NoGun,
		NoBlend,
		Recording,
		PlayerUpdateRequests,
		Fps
	}

	public class Setting : IClientCommand
	{
		public ClientCommand Type { get { return ClientCommand.Setting; } }

		public SettingType SubType { get; set; }
		public short Value { get; set; }

		//[short setting, short value]
		public Setting(ReadRawData data)
		{
			SubType = (SettingType)data.ReadShort();
			Value = data.ReadShort();
		}
		public Setting(SettingType subType, short value)
		{
			SubType = subType;
			Value = value;
		}

		#region ICommand
		public int Size()
		{
			return 5; // type + 2*short
		}

		public void WriteTo(WriteRawData package)
		{
			package.WriteByte((byte)Type);
			package.WriteShort((short)SubType);
			package.WriteShort(Value);
		}
		#endregion
	}
}
