using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Quake2Client
{
	public static class Settings
	{
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		public static void WriteValue(string path, string section, string key, string value)
		{
			WritePrivateProfileString(section, key, value, path + "\\Settings.ini");
		}

		public static string ReadValue(string path, string section, string key)
		{
			StringBuilder temp = new StringBuilder(255);
			GetPrivateProfileString(section, key, "", temp, 255, path + "\\Settings.ini");
			return temp.ToString();
		}
	}
}