using System;

namespace Jv.Networking
{
	public class ProxyException : Exception
	{
		public ProxyException(IProxy proxy, string message) : base(message)
		{
			Proxy = proxy;
		}

		public IProxy Proxy { get; private set; }
	}
}
