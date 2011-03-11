using System;
using Quake2.Client.Events;
using Quake2.Network.Commands;
using Quake2.Network.Commands.Server;
using Quake2.Network;
using Quake2.Extensions;
using Quake2.Network.Commands.Client;

namespace Quake2.Client
{
	public partial class Q2Client
	{
		public event EventHandler OnClear;

		#region Server
		public event ServerConnection.StringHandler OnServerConnectionlessPrint;
		public event ConnectionlessCommandEventHandler OnServerUnknownConnectionlessCommand;

		public event PreviewCommandEventHandler<Package<IServerCommand>> OnServerPackage;
		public event PreviewCommandEventHandler<IServerCommand> OnServerCommand;

		public event CommandEventHandler<IServerStringPackage> OnServerStringPackage;
		public event CommandEventHandler<ServerData> OnServerData;
		public event CommandEventHandler<CenterPrint> OnServerCenterPrint;
		public event CommandEventHandler<Print> OnServerPrint;
		public event CommandEventHandler<StuffText> OnServerStuffText;
		public event CommandEventHandler<ConfigString> OnServerConfigString;
		public event CommandEventHandler<PlayerInfo> OnServerPlayerInfo;
		public event CommandEventHandler<Layout> OnServerLayout;

		public event CommandEventHandler<Disconnect> OnServerDisconnect;
		#endregion

		#region Client
		//TODO: Ativar eventos de client
		public event PreviewCommandEventHandler<Package<IClientCommand>> OnClientPackage;
		public event PreviewCommandEventHandler<IClientCommand> OnClientCommand;

		public event PreviewCommandEventHandler<IClientStringPackage> OnClientStringPackage;
		public event PreviewCommandEventHandler<StringCmd> OnClientStringCmd;
		public event PreviewCommandEventHandler<UserInfo> OnClientUserInfo;
		public event PreviewCommandEventHandler<Setting> OnClientSetting;
		#endregion

		internal void FireEvents(Package<IServerCommand> serverPackage)
		{
			if (!OnServerPackage.Check(this, serverPackage))
				return;

			foreach (IServerCommand cmd in serverPackage.Commands)
			{
				if (!OnServerCommand.Check(this, cmd))
					continue;

				switch (((IServerCommand)cmd).Type)
				{
					case ServerCommand.Disconnect:
						OnServerDisconnect.Fire(this, (Disconnect)cmd);
						break;

					case ServerCommand.ServerData:
						OnServerData.Fire(this, (ServerData)cmd);
						break;

					case ServerCommand.CenterPrint:
						OnServerCenterPrint.Fire(this, (CenterPrint)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.Print:
						OnServerPrint.Fire(this, (Print)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.StuffText:
						OnServerStuffText.Fire(this, (StuffText)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.Layout:
						OnServerLayout.Fire(this, (Layout)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.ConfigString:
						switch (((ConfigString)cmd).ConfigType)
						{
							case ConfigStringType.PlayerInfo:
								OnServerPlayerInfo.Fire(this, (PlayerInfo)cmd);
								OnServerConfigString.Fire(this, (PlayerInfo)cmd);
								OnServerStringPackage.Fire(this, (PlayerInfo)cmd);
								break;

							default:
								OnServerConfigString.Fire(this, (ConfigString)cmd);
								OnServerStringPackage.Fire(this, (ConfigString)cmd);
								break;
						}
						break;
				}
			}
		}

		internal bool FireEvents(IClientCommand clientPackage)
		{
			if (!OnClientCommand.Check(this, clientPackage))
				return false;

			switch (((IClientCommand)clientPackage).Type)
			{
				case ClientCommand.UserInfo:
					if (!OnClientStringPackage.Check(this, (IClientStringPackage)clientPackage))
						return false;
					if (!OnClientUserInfo.Check(this, (UserInfo)clientPackage))
						return false;
					break;
				case ClientCommand.StringCmd:
					if (!OnClientStringCmd.Check(this, (StringCmd)clientPackage))
						return false;
					break;
				case ClientCommand.Setting:
					if (!OnClientStringPackage.Check(this, (IClientStringPackage)clientPackage))
						return false;
					if (!OnClientSetting.Check(this, (Setting)clientPackage))
						return false;
					break;
			}

			return true;
		}
	}
}
