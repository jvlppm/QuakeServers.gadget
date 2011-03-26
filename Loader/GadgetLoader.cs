using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Update;

namespace Loader
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

		private static byte[] InUseDll { get; set; }
		private static byte[] InUseZip { get; set; }
		private static object InUseWrapper { get; set; }

		private static byte[] LatestDll { get; set; }
		private static byte[] LatestZip { get; set; }
		private static object LatestWrapper { get; set; }

		public bool WrapperLoaded { get { return InUseWrapper != null; } }

		#endregion

		private static bool LocalUpdate { get; set; }
		public bool CanUpdate { get; private set; }

		public double LastUpdateTime { get; private set; }
		private DateTime _updateStart;

		public void CheckLocalUpdates(string gadgetPath, string dllSubPath, string className)
		{
			if(CanUpdate)
				return;

			string dllPath = gadgetPath + "\\" + dllSubPath;// @"\en-US\bin\Quake2Client.dll";

			if (!File.Exists(dllPath))
			{
				CanUpdate = false;
				return;
			}

			LatestDll = File.ReadAllBytes(dllPath);
			if (InUseDll != null && !InUseDll.SameAs(LatestDll))
			{
				lock (AssemblyLock)
				{
					LocalUpdate = true;
					CanUpdate = true;
					PreLoadType(LatestDll, className);
					LastUpdateTime = 0;
				}
			}
		}

		private bool _updating;

		public void CheckUpdates(string gadgetPath, string dllSubPath, string className)
		{
			if (_updating || LocalUpdate)
				return;

			_updateStart = DateTime.Now;
			_updating = true;

			var wrapperUpdater = new Thread((ThreadStart)delegate
			{
				try
				{
					const string zipUrl = @"https://github.com/jvlppm/Bin/raw/master/QuakeServers.gadget";
					string dllPath = gadgetPath + "\\" + dllSubPath;// @"\en-US\bin\Quake2Client.dll";

					var newZip = WebClient.DownloadData(zipUrl);
					if (!InUseZip.SameAs(newZip))
						UpdateLatestVersion(gadgetPath, newZip, dllPath, className);

					LastUpdateTime = DateTime.Now.Subtract(_updateStart).TotalSeconds;
				}
				catch { }
				finally
				{
					_updating = false;
				}
			});

			wrapperUpdater.Start();
		}

		private void UpdateLatestVersion(string gadgetPath, byte[] newZip, string dllPath, string className)
		{
			if (!File.Exists(dllPath))
			{
				CanUpdate = false;
				return;
			}

			lock (AssemblyLock)
			{
				const string tempZipFileName = "lastVersion.zip";

				File.WriteAllBytes(tempZipFileName, newZip);
				ExtractZipTo(tempZipFileName, gadgetPath, true);
				LatestDll = File.ReadAllBytes(dllPath);
				PreLoadType(LatestDll, className);
				LatestZip = newZip;
				CanUpdate = true;
			}
		}

		private static void ExtractZipTo(string zipPath, string gadgetPath, bool deleteZip)
		{
			Zip.UnZipFiles(zipPath, gadgetPath, null, deleteZip);
		}

		static void PreLoadType(byte[] dll, string className)
		{
			//const string className = "Quake2Client.Quake2Client";
			var assembly = Assembly.Load(dll);
			LatestWrapper = Activator.CreateInstance(assembly.GetType(className));
		}

		public object Update()
		{
			if (CanUpdate)
			{
				lock (AssemblyLock)
				{
					CanUpdate = false;
					InUseWrapper = LatestWrapper;
					InUseZip = LatestZip;
					InUseDll = LatestDll;
					LatestZip = null;
					LatestWrapper = null;
					LatestDll = null;
				}
			}

			return InUseWrapper;
		}
	}
}
