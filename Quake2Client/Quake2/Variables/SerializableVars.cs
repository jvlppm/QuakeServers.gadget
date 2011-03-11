using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Quake2.Variables
{
	public class SerializableVars : VarCollection
	{
		public SerializableVars(string serialized)
		{
			SerializedVars = serialized;
		}
		public SerializableVars() { }

		public override void Add(Variable item)
		{
			if (item.Value.ToString().Contains("\""))
				throw new Exception("Invalid variable value ('\"')");

			base.Add(item);
		}

		public override void SetVar(string name, object value)
		{
			if (value.ToString().Contains("\""))
				throw new Exception("Invalid variable value ('\"')");

			base.SetVar(name, value);
		}

		public string SerializedVars
		{
			get
			{
				return ((ICollection<Variable>)this).Aggregate(string.Empty, (m, v) => m + "\\" + v.Name + "\\" + (v.Value is IFormattable? ((IFormattable)v.Value).ToString(null, System.Globalization.CultureInfo.InvariantCulture):v.Value));
			}
			set
			{
				string regex = @"\\([^\\]+)\\([^\\]*)";
				if (!Regex.IsMatch(value, regex))
					throw new Exception("value must match " + regex);

				VarCollection newVars = new VarCollection();

				foreach (Match match in Regex.Matches(value, regex))
				{
					newVars.Add(match.Groups[1].Value, match.Groups[2].Value);
					SetVar(match.Groups[1].Value, match.Groups[2].Value);
				}

				foreach (var ex in ((ICollection<Variable>)this).Where(e => !newVars.Contains(e.Name)))
					Remove(ex);
			}
		}
	}
}
