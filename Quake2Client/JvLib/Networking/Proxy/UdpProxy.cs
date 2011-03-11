using System;
using System.Net;
using System.Net.Sockets;
using Jv.Threading;

namespace Jv.Networking
{
	public class UdpProxy : IProxy
	{
		#region Events
		public event MessageEventHandler OnSourceMessage;
		public event MessageEventHandler OnDestinationMessage;
		public event ErrorEventHandler OnError;
		#endregion

		#region Attributes
		UdpClient _sourceSock;
		UdpClient _destinationSock;
		ConnectionPoint _source, _destination;
		#endregion

		public void ForwardLocalConnections(int localPort, ConnectionPoint destination)
		{
			ForwardConnections(new ConnectionPoint(new IPEndPoint(IPAddress.Loopback, localPort), false), destination);
		}

		public void ForwardConnections(ConnectionPoint source, ConnectionPoint destination)
		{
			if(_source != null)
				throw new ProxyException(this, "Proxy already started");
			
			_source = source;
			_destination = destination;

			if(!_source.OpenConnection)
			{
				_sourceSock = new UdpClient(_source.EndPoint);
			}
			else
			{
				_sourceSock = new UdpClient();
				_sourceSock.Connect(_source.EndPoint);
			}

			if(!_destination.OpenConnection)
			{
				_destinationSock = new UdpClient(_destination.EndPoint);
			}
			else
			{
				_destinationSock = new UdpClient();
				_destinationSock.Connect(_destination.EndPoint);
			}

			Parallel.Start("UDP: source->destination", () =>
			{
				try
				{
					while (true)
					{
						var e = new MessageEventArgs(_sourceSock.Receive(ref _source.EndPoint));

						if (OnSourceMessage != null)
						{
							try { OnSourceMessage(this, e); }
							catch { }
						}

						SendToDestination(e.Data);
					}
				}
				catch
				{
					_destinationSock.Close();
				}
			});

			Parallel.Start("UDP: destination->source", () =>
			{
				try
				{
					while (true)
					{
						var e = new MessageEventArgs(_destinationSock.Receive(ref _destination.EndPoint));

						if (OnDestinationMessage != null)
						{
							try { OnDestinationMessage(this, e); }
							catch { }
						}
						SendToSource(e.Data);
					}
				}
				catch (Exception ex)
				{
					_source = null;

					if (OnError != null)
					{
						try { OnError(this, new ErrorEventArgs(ex)); }
						catch { }
					}
				}
			});
		}

		public void SendToSource(byte[] data)
		{
			if (_source == null)
				throw new ProxyException(this, "Proxy was not started");
			if (data == null)
				return;

			lock (_sourceSock)
			{
				if (_source.OpenConnection)
					_sourceSock.Send(data, data.Length);
				else
					_sourceSock.Send(data, data.Length, _source.EndPoint);
			}
		}

		public void SendToDestination(byte[] data)
		{
			if (_source == null)
				throw new ProxyException(this, "Proxy was not started");
			if (data == null)
				return;

			lock (_destinationSock)
			{
				if (_destination.OpenConnection)
					_destinationSock.Send(data, data.Length);
				else
					_destinationSock.Send(data, data.Length, _destination.EndPoint);
			}
		}
	}
}
