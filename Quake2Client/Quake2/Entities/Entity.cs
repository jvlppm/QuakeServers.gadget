using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Quake2.Entities
{
	public class Entity
	{
		public static string FieldRegex { get { return "\\s*\"([^\"]*)\"\\s*\"([^\"]*)\"\\s*"; } }
		public static string FormatRegex { get { return "{(" + FieldRegex + ")*}"; } }

		public IDictionary<string, object> KeyValues { get; private set; }

		public Entity()
		{
			KeyValues = new Dictionary<string, object>();
		}

		public string ClassName { get { return KeyValues["classname"].ToString(); } }

		public static Entity Parse(string serialized)
		{
			#region Regex
			string classNameRegex = "\"classname\"\\s*\"([^\"]*)\"";

			if (!Regex.IsMatch(serialized, FormatRegex))
				throw new Exception("Bad entity format");

			var classNameResults = Regex.Matches(serialized, classNameRegex);
			if (classNameResults.Count != 1)
				throw new Exception("Invalid classname");

			string classname = classNameResults[0].Groups[1].ToString();

			var matches = from m in Regex.Matches(serialized, FieldRegex).Cast<Match>()
						  where m.Groups[1].ToString() != "classname"
						  select new
						  {
							  Name = m.Groups[1].ToString(),
							  Value = m.Groups[2].ToString()
						  };

			#endregion

			Entity newEntity = null;

			switch (classname)
			{
				case "worldspawn":
					newEntity = new WorldSpawn();
					foreach (var m in matches)
						((WorldSpawn)newEntity).Set(m.Name, m.Value);
					break;

				case "target_speaker":
					newEntity = new TargetSpeaker();
					foreach (var m in matches)
						((TargetSpeaker)newEntity).Set(m.Name, m.Value);
					break;

				default:
					newEntity = new Entity();
					foreach (var m in matches)
						newEntity.KeyValues.Add(m.Name, m.Value);
					break;
			}
			newEntity.KeyValues.Add("classname", classname);
			return newEntity;
		}

		#region ToString
		public override string ToString()
		{
			StringBuilder serialized = new StringBuilder();

			serialized.AppendLine("{");

			foreach (var kv in KeyValues)
				serialized.AppendFormat("\"{0}\" \"{1}\"\n", kv.Key, kv.Value);

			serialized.AppendLine("}");

			return serialized.ToString();
		}

		public override int GetHashCode()
		{
			return ClassName.GetHashCode();
		}
		#endregion
	}
}
