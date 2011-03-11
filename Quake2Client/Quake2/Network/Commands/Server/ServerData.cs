using System;
using Jv.Networking;

namespace Quake2.Network.Commands.Server
{
	public class ServerData : IServerCommand
	{
		public Protocol Protocol { get; set; }
		public int ServerCount { get; set; }
		public bool CinematicOrDemo { get; set; }
		public string GameDir { get; set; }
		public short PlayerNum { get; set; }
		public string LevelName { get; set; }
		public byte StrafeHack { get; set; }
		public short R1Q2ProtocolVersion { get; set; }
		public bool EnhancedVersion { get; set; }
		public byte Deltas { get; set; } //r1q2: was adv.deltas

		public ServerData() { }

		//[int protocol][int serverCount][byte attractLoop][string gameDir][string levelName] ...
		public ServerData(ReadRawData serverPackage)
		{
			Protocol = (Protocol)serverPackage.ReadInt();
			ServerCount = serverPackage.ReadInt();
			CinematicOrDemo = serverPackage.ReadByte() != 0;
			GameDir = serverPackage.ReadString();
			PlayerNum = serverPackage.ReadShort();
			LevelName = serverPackage.ReadString();

			if (Protocol == Protocol.R1Q2)
			{
				EnhancedVersion = serverPackage.ReadByte() != 0;
				if (EnhancedVersion)
					throw new Exception("Protocol not supported (Enhanced r1q2)");
				R1Q2ProtocolVersion = serverPackage.ReadShort();
				if (R1Q2ProtocolVersion >= 1903)
				{
					Deltas = serverPackage.ReadByte();
					StrafeHack = serverPackage.ReadByte();
				}
			}
		}

		#region ICommand
		public int Size()
		{
			int size = 12;
			if (!string.IsNullOrEmpty(GameDir))
				size += GameDir.Length + 1;
			else size++;

			if (!string.IsNullOrEmpty(LevelName))
				size += LevelName.Length + 1;
			else size++;

			if (Protocol == Protocol.R1Q2)
			{
				size += 3;
				if (R1Q2ProtocolVersion >= 1903)
					size += 2;
			}

			return size;
		}

		public void WriteTo(WriteRawData data)
		{
			data.WriteByte((byte)Type);
			data.WriteInt((int)Protocol);
			data.WriteInt(ServerCount);
			data.WriteByte(CinematicOrDemo ? (byte)1 : (byte)0);
			data.WriteString(GameDir);
			data.WriteShort(PlayerNum);
			data.WriteString(LevelName);

			if (Protocol != Protocol.R1Q2)
				return;

			data.WriteByte(EnhancedVersion ? (byte)1 : (byte)0);
			data.WriteShort(R1Q2ProtocolVersion);

			if (R1Q2ProtocolVersion < 1903)
				return;

			data.WriteByte(Deltas);
			data.WriteByte(StrafeHack);
		}

		public ServerCommand Type { get { return ServerCommand.ServerData; } }
		#endregion
	}
}
