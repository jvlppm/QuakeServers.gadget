using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Update;

namespace Wrapper
{
	static class ArrayExtensions
	{
		public static bool SameAs<TArrayType>(this TArrayType[] array, TArrayType[] compareTo)
		{
			if ((compareTo == null) != (array == null))
				return false;

			if (array == null)
				return true;

			if (array.Length != compareTo.Length)
				return false;

			return !array.Where((t, i) => !t.Equals(compareTo[i])).Any();
		}
	}

	[ComVisible(true)]
	public class GadgetLoader
	{
		#region Static
		static object _assemblyLock;
		static object AssemblyLock
		{
			get { return _assemblyLock ?? (_assemblyLock = new object()); }
		}

		static WebClient _webClient;
		static WebClient WebClient
		{
			get { return _webClient ?? (_webClient = new WebClient()); }
		}

		private static byte[] InUseZip { get; set; }
		private static object InUseWrapper { get; set; }

		private static byte[] LatestZip { get; set; }
		private static object LatestWrapper { get; set; }

		public bool WrapperLoaded { get { return InUseWrapper != null; } }

		#endregion

		public bool CanUpdate { get; private set; }

		public double LastUpdateTime { get; private set; }
		private DateTime UpdateStart;

		public void CheckUpdates(string gadgetPath)
		{
			UpdateStart = DateTime.Now;

			string zipUrl = @"https://github.com/jvlppm/Bin/raw/master/QuakeServers.gadget.zip";

			if (File.Exists(@"D:\Documents\Projects\Old\Bin\QuakeServers.gadget.zip"))
				zipUrl = @"D:\Documents\Projects\Old\Bin\QuakeServers.gadget.zip";

			string dllPath = gadgetPath + @"\Quake2Client\bin\Quake2Client.dll";

			if (!zipUrl.StartsWith("http"))
			{
				var newZip = File.ReadAllBytes(zipUrl);
				if (!InUseZip.SameAs(newZip))
				{
					ExtractZipTo(zipUrl, gadgetPath + "\\..", false);
					LoadType(File.ReadAllBytes(dllPath), "Quake2Client.Quake2Client");
					InUseZip = newZip;
					CanUpdate = true;
					LastUpdateTime = DateTime.Now.Subtract(UpdateStart).TotalSeconds;
				}
			}
			else
			{
				var wrapperUpdater = new Thread((ThreadStart)delegate
				{
					const string tempZipFileName = "lastVersion.zip";
					var newZip = WebClient.DownloadData(zipUrl);
					if (!InUseZip.SameAs(newZip))
					{
						File.WriteAllBytes(tempZipFileName, newZip);
						ExtractZipTo(tempZipFileName, gadgetPath + "\\..", true);
						LoadType(File.ReadAllBytes(dllPath), "Quake2Client.Quake2Client");
						InUseZip = newZip;
						CanUpdate = true;
						LastUpdateTime = DateTime.Now.Subtract(UpdateStart).TotalSeconds;
					}
				});

				wrapperUpdater.Start();
			}
		}

		private static void ExtractZipTo(string zipPath, string gadgetPath, bool deleteZip)
		{
			Zip.UnZipFiles(zipPath, gadgetPath, null, deleteZip);
		}

		static void LoadType(byte[] dll, string className)
		{
			lock (AssemblyLock)
			{
				var assembly = Assembly.Load(dll);
				LatestWrapper = Activator.CreateInstance(assembly.GetType(className));
			}
		}

		public object Update()
		{
			if (CanUpdate)
			{
				lock (AssemblyLock)
				{
					CanUpdate = false;
					InUseWrapper = LatestWrapper;
					LatestWrapper = null;
				}
			}

			return InUseWrapper;
		}
	}
}
