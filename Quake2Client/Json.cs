using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Collections;

namespace Quake2Client
{
	public static class Json
	{
		public static string Extract(object obj)
		{
			return Extract(obj, false, 0);
		}

		public static string Extract(object obj, bool ident)
		{
			return Extract(obj, ident, 0);
		}

		public static string Extract(object obj, bool ident, int currentIdentation)
		{
			if ((object)obj == null)
				return "null";

			if (obj is bool)
				return ((bool)obj).ToString().ToLower();

			if (obj is int || obj is Int64 || obj is decimal || obj is double || obj is float)
				return ((IConvertible)obj).ToString(CultureInfo.InvariantCulture);

			if (obj is string || obj is char || obj is Enum)
				return "\"" + EncodeString(obj.ToString()) + "\"";

			if (obj is IDictionary<string, object>)
			{
				StringBuilder json = new StringBuilder();
				if (ident)
					json.AppendLine();
				json.Append(new string('\t', currentIdentation));
				json.Append("{");

				bool first = true;

				if (ident)
					currentIdentation++;

				foreach (var key in (obj as IDictionary<string, object>).Keys)
				{
					if (first) first = false;
					else json.Append(',');

					if (ident)
					{
						json.AppendLine();
						json.Append(new string('\t', currentIdentation));
					}

					json.AppendFormat(ident ? "\"{0}\": {1}" : "\"{0}\":{1}", key, Extract((obj as IDictionary<string, object>)[key], ident, currentIdentation));
				}

				if (ident)
				{
					currentIdentation--;
					json.AppendLine();
					json.Append(new string('\t', currentIdentation));
				}

				json.Append("}");

				return json.ToString();
			}

			else if (obj is IEnumerable)
			{
				StringBuilder json = new StringBuilder();
				if (ident)
					json.AppendLine();
				json.Append(new string('\t', currentIdentation));
				json.Append("[");

				bool first = true;

				if (ident)
					currentIdentation++;

				foreach (var value in (obj as IEnumerable))
				{
					if (first) first = false;
					else json.Append(',');

					if (ident)
					{
						json.AppendLine();
						json.Append(new string('\t', currentIdentation));
					}

					json.Append(Extract(value, ident, currentIdentation));
				}

				if (ident)
				{
					currentIdentation--;
					json.AppendLine();
					json.Append(new string('\t', currentIdentation));
				}

				json.Append("]");

				return json.ToString();
			}

			Dictionary<string, object> extractedInfo = new Dictionary<string, object>();
			foreach (PropertyInfo prop in obj.GetType().GetProperties())
				extractedInfo.Add(prop.Name, prop.GetValue(obj, null));
			return Extract(extractedInfo, ident, currentIdentation);
		}

		static string EncodeString(string original)
		{
			StringBuilder final = new StringBuilder();
			foreach (char ch in original)
			{
				if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-+_.,~^ ()[]{}%@/!?#&*:".IndexOf(ch) < 0)
					final.Append("\\u" + ((int)ch).ToString("X4"));
				else final.Append(ch);
			}

			return final.ToString();
		}
	}
}
