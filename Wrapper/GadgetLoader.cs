using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

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

		private static byte[] InUseAssembly { get; set; }
		//public static object Wrapper { get; private set; }

		private static byte[] LatestAssembly { get; set; }
		private static object LatestWrapper { get; set; }

		#endregion

		public bool CanUpdate { get; private set; }

		public void Load(string url, string className)
		{
			if (!url.StartsWith("http"))
				Load(File.ReadAllBytes(url), className);
			else
			{
				var wrapperUpdater = new Thread((ThreadStart)delegate
				{
					var dll = WebClient.DownloadData(url);
					Load(dll, className);
				});

				wrapperUpdater.Start();
			}
		}

		public void Load(byte[] dll, string className)
		{
			if (dll.SameAs(LatestAssembly) || dll.SameAs(InUseAssembly))
				return;

			lock (AssemblyLock)
			{
				var assembly = Assembly.Load(dll);

				LatestWrapper = Activator.CreateInstance(assembly.GetType(className));
				LatestAssembly = dll;
				CanUpdate = true;
			}
		}

		public object Update()
		{
			object latestWrapper;

			lock (AssemblyLock)
			{
				if (!CanUpdate)
					throw new Exception("No update available");

				CanUpdate = false;
				InUseAssembly = LatestAssembly;
				latestWrapper = LatestWrapper;
				LatestAssembly = null;
				LatestWrapper = null;
			}

			return latestWrapper;
		}
	}
}
