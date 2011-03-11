using System.Collections.Generic;
//using System.Dynamic;
using Quake2.Client.Events;
using Quake2.Network.Commands.Client;
using Quake2.Variables;

namespace Quake2.Client
{
	public partial class Q2Client
	{
		public VarCollection Info { get; private set; } // Data maintened by the client
		public Dictionary<string, VarCollection> UserVars { get; private set; } // Data maintained by the user

		internal UserInfo UserInfo { get; set; } // Data exposed to the server
		VarCollection ServerVars { get; set; } // Data set by the server
		Dictionary<string, string> ServerAliases { get; set; } // Aliases set by the server

		void SetupVars()
		{
			#region Local
			Info = new VarCollection();
			Info.SetMember("Version", Quake2.Version);
			Info.SetMember("CurrentServer", null);

			UserVars = new Dictionary<string, VarCollection>
			           	{
			           		{"User", new VarCollection()},
							{"Net", new VarCollection()}
			           	};

			UserVars["User"].SetMember("Spectator", false);
			UserVars["User"].SetMember("Name","Player");
			UserVars["User"].SetMember("Password", string.Empty);
			UserVars["User"].SetMember("Hand", 0);
			UserVars["User"].SetMember("Fov", 90);

			
			UserVars["Net"].SetMember("PacketDup", 0);
			UserVars["Net"].SetMember("FlushSendBufferTime", 10);
			#endregion

			#region Server
			ServerAliases = new Dictionary<string, string>();
			ServerVars = new VarCollection
			{
				{"version", Info.GetMember("Version")},

				{"gl_driver", "opengl32"},
				{"cl_anglespeedkey", 1.5},
				{"cl_maxfps", 100},

				{"timescale", 1.0},
				{"cl_pitchspeed", 150},

				// UserInfo
				{"password", UserVars["User"].GetMember("Password")},
				{"spectator", new MapBoolToInt((Variable)UserVars["User"].GetMember("Spectator"))},
				{"name", UserVars["User"].GetMember("Name")},
				{"skin", "male/cop"},
				{"rate", 25000},
				{"msg", 1},
				{"hand", UserVars["User"].GetMember("Hand")},
				{"fov", 90},
				{"gender", "male"}
			};

			// UserInfo
			UserInfo = new UserInfo();
			UserInfo.UseVar(ServerVars.GetVar("password"));
			UserInfo.UseVar(ServerVars.GetVar("spectator"));
			UserInfo.UseVar(ServerVars.GetVar("name"));
			UserInfo.UseVar(ServerVars.GetVar("skin"));
			UserInfo.UseVar(ServerVars.GetVar("rate"));
			UserInfo.UseVar(ServerVars.GetVar("msg"));
			UserInfo.UseVar(ServerVars.GetVar("hand"));
			UserInfo.UseVar(ServerVars.GetVar("fov"));
			UserInfo.UseVar(ServerVars.GetVar("gender"));

			UserInfo.OnValueChanged += (s, e) => UserInfoModified = true;
			#endregion
		}

		internal bool UserInfoModified { get; set; }
	}
}
