using System.Collections.Generic;
using Quake2.Variables;

namespace Quake2.Extensions
{
	public static class StringExtensions
	{
		public static string ToLowerId(this string name)
		{
			return name.ToLower();
			//return Regex.Replace(name, "[a-z][A-Z]", m => m.Value[0] + "_" + m.Value[1]).ToLower();
		}

		public static string Escape(this string command)
		{
			var slashSequences = new Dictionary<string, string>
			{
				{"\\", "\\\\"},
				{"\n", "\\n"},
				{"\'", "\\\'"},
				{"\"", "\\\""},
				{"\0", "\\0"},
				{"\a", "\\a"},
				{"\b", "\\b"},
				{"\f", "\\f"},
				{"\r", "\\r"},
				{"\t", "\\t"},
				{"\v", "\\v"}
			};

			string final = command;

			foreach (var escape in slashSequences)
				final = final.Replace(escape.Key, escape.Value);

			return final;
		}

		public static string ReplaceVars(this string command, VarCollection vars)
		{
			bool quoted = false;

			for (int i = 0; i < command.Length; i++)
			{
				if (command[i] == '\"')
					quoted = !quoted;
				if (!quoted && command[i] == '$')
				{
					string vr = string.Empty;
					for (int j = i + 1; j < command.Length && command[j] != ' ' && command[j] != ';' && command[j] != '\n'; j++)
						vr += command[j];

					if (vars.GetVar(vr) != null)
					{
						string value = vars.GetVar(vr);
						command = command.Replace("$" + vr, value);
						i += value.Length;
					}
					else
						command = command.Replace("$" + vr, string.Empty);
				}
			}

			return command;
		}

		public static string GetToken(this string command, int pos)
		{
			int i;
			return command.GetToken(pos, out i);
		}

		public static string GetToken(this string command, int pos, out int i)
		{
			bool inQuotes = false;
			string token = string.Empty;
			bool wait;

			for (i = 0; i < command.Length; i = wait ? i : i + 1)
			{
				wait = false;
				char ch = command[i];
				if (inQuotes)
				{
					if (ch == '\"')
						inQuotes = false;
					else token += ch;
				}
				else
				{
					switch (ch)
					{
						case '\r':
						case '\n':
						case ';':

							if (token == string.Empty)
							{
								token += ";";
								i++;
							}
							else wait = true;
							goto case ' ';

						case '\t':
						case ' ':
							if (token != string.Empty)
							{
								if (pos > 0)
								{
									pos--;
									token = string.Empty;
								}
								else
									return token;
							}
							break;
						default:
							if (ch == '\"') inQuotes = true;
							else token += ch;
							break;
					}
				}
			}

			return token;
		}
	}
}
