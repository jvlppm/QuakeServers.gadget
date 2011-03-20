using System;
using System.Collections.Generic;
using Quake2.Client.Events;
using Quake2.Extensions;
using Quake2.Variables;

namespace Quake2.Client
{
	public partial class Q2Client
	{
		public event ExecuteMethodEventHandler OnExecuteMethod;

		public Dictionary<string, Action<string>> ServerMethods { get; private set; }

		void SetupMethods()
		{
			ServerMethods = new Dictionary<string, Action<string>>
			{
				{ "connect", s =>
					{
						string[] host = s.Substring(s.IndexOf(' ') + 1).Split(':');
						if(host.Length == 2)
							Connect(host[0], int.Parse(host[1]));
					}
				},
				{ "precache", s =>
					{
						Precache = int.Parse(s.Substring(s.IndexOf(' ')));
						StuffCommand("begin " + Precache);
					}
				},
				{ "cmd", s => { lock (UserInfoLock) StuffCommand(s.Substring(4)); }},
				{ "set", s =>
						{
							string[] words = s.Split(' ');
							string varName = words[1];
							Set(varName, words[2], words.Length > 3 ? words[3] : null);
						}
				},
				{ "alias", s =>
						{
							string var = s.GetToken(1).ToLower();
							if (!ServerAliases.ContainsKey(var))
								ServerAliases.Add(var, s.GetToken(2));
							else
								ServerAliases[var] = s.GetToken(2);
						}
				},
				{ "echo", s => {} },//TODO: ECHO
				{ "wait", s => {} },
				{ "rate", s => Set("rate", s.Split(' ')[1], null) },
				{ "packetdup", s => { UserVars["Net"].SetMember("PacketDup", uint.Parse(s.GetToken(1))); } },
				{ "clear",
					s => { if(OnClear != null) OnClear(this, EventArgs.Empty); }
				},
				{ "name", s => { ServerVars.SetVar("name", s.GetToken(1)); }}
			};
		}

		public void Set(string name, string value, string flag)
		{
			int intValue;
			double doubleValue;
			if (int.TryParse(value, out intValue))
				((VarCollection)ServerVars).SetVar(name, intValue);
			else if (double.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out doubleValue))
				((VarCollection)ServerVars).SetVar(name, doubleValue);
			else
				((VarCollection)ServerVars).SetVar(name, value);

			bool notUserVar = UserInfo.GetVar(name) == null;

			if (notUserVar && flag == "u")
			{
				if (string.IsNullOrEmpty(value))
					throw new Exception("Variable " + name + " cannot be empty");

				UserInfo.UseVar(ServerVars.GetVar(name));
				//((VarCollection)ServerVars).GetVar(name).OnValueChanged += UserInfoModified;

				if (Connection != null)
					Send(UserInfo);
			}
		}

		public void StuffText(string command)
		{
			string subCommand = string.Empty;
			do
			{
				string token = GetNextToken(ref command);

				if (token == ";")
				{
					StuffCommand(subCommand.TrimEnd(' '));
					subCommand = string.Empty;
				}
				else if (token != string.Empty)
					subCommand += token + " ";
			} while (command != string.Empty);

			if (subCommand != string.Empty)
				StuffCommand(subCommand.TrimEnd(' '));
		}

		void StuffCommand(string command)
		{
			var currentCommand = command.ReplaceVars((VarCollection)ServerVars).TrimEnd(' ');

			string cmd;
			bool cont;

			do
			{
				cont = false;
				cmd = currentCommand.GetToken(0).ToLower();

				if (ServerAliases.ContainsKey(cmd))
				{
					currentCommand = ServerAliases[cmd].ReplaceVars((VarCollection)ServerVars);
					cont = true;
				}
			} while (cont);

			if (ServerMethods.ContainsKey(cmd))
			{
				var eventArgs = new ExecuteMethodEventArgs(command, currentCommand);
				if (OnExecuteMethod != null)
					OnExecuteMethod(this, eventArgs);

				if (!eventArgs.Abort)
					ServerMethods[cmd](currentCommand);
			}
			else
				Send(currentCommand);
		}

		static string GetNextToken(ref string command)
		{
			int i;
			string token = command.GetToken(0, out i);
			command = command.Substring(i);

			return token;
		}
	}
}