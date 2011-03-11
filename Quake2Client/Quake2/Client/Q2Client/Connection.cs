using Quake2.Network.Commands.Client;

namespace Quake2.Client
{
	public partial class Q2Client
	{
		public ConnectionStatus ConnectionStatus
		{
			get
			{
				if (Connection == null)
					return ConnectionStatus.Disconnected;
				return Connection.Status;
			}
		}
		ServerConnection Connection { get; set; }

		public int Precache { get; private set; }

		void SetupConnection()
		{
			OnServerStuffText += (s, e) => StuffText(e.Command.Message);
		}

		public void Disconnect()
		{
			if (Connection != null)
				Connection.Disconnect();
		}

		public void Connect(string hostname, int port)
		{
			if (Connection == null || Connection.Port != port || Connection.Host != hostname)
			{
				if (Connection != null)
					Connection.Dispose();

				Info.SetMember("CurrentServer", hostname + ":" + port);

				Connection = new ServerConnection(this, hostname, port, 27901);
				Connection.OnPackage += (s, e) => FireEvents(e);

				Connection.OnServerConnectionlessPrint += (s, t) => OnServerConnectionlessPrint(s, t);
				Connection.OnServerUnknownConnectionlessCommand += (s, t) => { OnServerUnknownConnectionlessCommand(s, t); };
			}

			Connection.Connect();
		}

		public void Send(string cmd, bool reliable = true)
		{
			if (cmd == string.Empty)
				return;
			Send(new StringCmd(cmd), reliable);
		}

		public void Send(IClientCommand package, bool reliable = false)
		{
			if(FireEvents(package))
				Connection.Send(package, reliable);
		}
	}
}
