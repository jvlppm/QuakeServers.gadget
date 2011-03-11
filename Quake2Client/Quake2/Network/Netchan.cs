using System;
using System.Net;
using System.Threading;
using Jv.Networking;
using Quake2.Network.Commands;
using Quake2.Network.Commands.Client;

namespace Quake2.Network
{
	//Primeiro ID indica o ID do pacote atual, segundo ID indica o ultimo ID recebido.

	//O primeiro bit do primeiro ID indica se este pacote contém dados Reliable.
	//Ao receber um pacote reliable é invertido localmente o IncomingReliableSequence (0 / 1).

	//O primeiro bit do segundo ID contém o IncomingReliableSequence.

	public class Netchan<SendCommandType> where SendCommandType : ICommand
	{
		enum ChanType
		{
			LocalClient,
			LocalServer
		}

		enum PacketStatus
		{
			Unknown,
			Received,
			Lost
		}

		Package<SendCommandType> NonReliablePackage { get; set; }
		Package<SendCommandType> NextReliablePackage { get; set; }
		Package<SendCommandType> LastSentReliablePackage { get; set; }

		public IPEndPoint IP { get; private set; }
		public Protocol Protocol { get; private set; }

		public ushort QPort { get; private set; }
		ChanType LocalType { get; set; }

		double PacketSizeLimit { get; set; }

		bool LocalReliableSequence { get; set; }
		bool RemoteReliableSequence { get; set; }
		PacketStatus LastReliableStatus { get; set; }

		uint LastSentId { get; set; }
		uint LastReceivedId { get; set; }
		uint LastReliableSentId { get; set; }

		bool AckNeeded { get; set; }

		DateTime LastSent { get; set; } // for retransmits
		DateTime LastReceived { get; set; } // for timeouts

		public Netchan(IPEndPoint ip, Protocol protocol, ushort qPort)
		{
			NonReliablePackage = new Package<SendCommandType>();
			NextReliablePackage = new Package<SendCommandType>();
			LastSentReliablePackage = new Package<SendCommandType>();

			LastReliableStatus = PacketStatus.Received;

			IP = ip;
			Protocol = protocol;
			QPort = qPort;
			LocalType = typeof(IClientCommand).IsAssignableFrom(typeof(SendCommandType)) ? ChanType.LocalClient : ChanType.LocalServer;

			PacketSizeLimit = protocol == Protocol.R1Q2 ? double.PositiveInfinity : 1400;
		}

		public bool MatchQPort(ReadRawData package)
		{
			if (package.Data.Length < (Protocol == Protocol.R1Q2 ? 9 : 10))
				return false;

			int oldPos = package.CurrentPosition;
			bool match;

			package.CurrentPosition = 8;
			if (Protocol == Protocol.R1Q2)
				match = package.ReadByte() == QPort;
			else
				match = package.ReadShort() == QPort;

			if (!match)
				package.CurrentPosition = oldPos;

			return match;
		}

		public bool Accept(uint id, uint ack)
		{
			lock (this)
			{
				LastReceived = DateTime.Now;

				bool isReliablePacket = id >> 31 == 1;
				bool remoteReliableSequence = ack >> 31 == 1; // Remote copy of my reliable sequence

				//if (isReliablePacket)
				AckNeeded = true;

				id &= ~(1 << 31);
				ack &= ~(1 << 31);

				if (id <= LastReceivedId)
					return false;

				LastReceivedId = id;

				//uint DroppedPackets = sequence - (LasrReceivedSequence + 1);

				if (LastReliableStatus == PacketStatus.Unknown)
				{
					if (ack >= LastReliableSentId)
					{
						if (remoteReliableSequence == LocalReliableSequence)
						{
							LastSentReliablePackage.Clear(); // LastSentReliablePackage received
							LastReliableStatus = PacketStatus.Received;
						}
						else
						{
							LastReliableStatus = PacketStatus.Lost;
							LocalReliableSequence = remoteReliableSequence;
						}
					}
				}
				//else { LastReliableStatus = PacketStatus.Unknown; }

				if (isReliablePacket)
					RemoteReliableSequence = !RemoteReliableSequence;

				return true;
			}
		}

		public void Send(SendCommandType command, bool reliable = false)
		{
			if (reliable)
			{
				lock (NextReliablePackage)
				{
					if (!NextReliablePackage.Commands.Contains(command))
						NextReliablePackage.Commands.Enqueue(command);
				}
			}
			else
			{
				lock (NonReliablePackage)
				{
					if (Protocol >= Protocol.R1Q2 || 8 + WriteHeaderAdditionalSize + NonReliablePackage.Size() + command.Size() <= PacketSizeLimit)
						NonReliablePackage.Commands.Enqueue(command);
				}
			}
		}

		public bool PendingReliable
		{
			get
			{
				lock (this)
				{
					return NextReliablePackage.Commands.Count != 0 || LastReliableStatus != PacketStatus.Received;
				}
			}
		}

		public WriteRawData GetNextPacket()
		{
			/*
			 * Se o último reliable foi recebido, e existe novo reliable, envia-lo.
			 * Se o último reliable foi perdido, atualizar reliable e reenviar.
			 * Se o último reliable nao foi confirmado:
			 *	Enviar próximo não reliable. // OU Reenviar último reliable ?
			 * */
			bool sendingReliable = false, sendAck;
			Package<SendCommandType> sending = null;
			uint header1, header2;

			lock (this)
			{
				if (LastReliableStatus != PacketStatus.Unknown)
				{
					/*
					 * Pegar todos os comandos em reliable, passar pro LastSentReliable e enviar.
					 * */
					lock (NextReliablePackage)
					{
						if (Protocol >= Protocol.R1Q2)
						{
							while (NextReliablePackage.Commands.Count > 0)
								LastSentReliablePackage.Commands.Enqueue(NextReliablePackage.Commands.Dequeue());
						}
						else
						{
							int nextSize = 8 + WriteHeaderAdditionalSize;
							while (NextReliablePackage.Commands.Count > 0)
							{
								nextSize += NextReliablePackage.Commands.Peek().Size();
								if (nextSize >= PacketSizeLimit)
									break;
								LastSentReliablePackage.Commands.Enqueue(NextReliablePackage.Commands.Dequeue());
							}
						}
					}

					if (LastSentReliablePackage.Commands.Count > 0)
					{
						sendingReliable = true;
						sending = LastSentReliablePackage;
						LocalReliableSequence = !LocalReliableSequence;
					}
				}

				LastSent = DateTime.Now;
				LastSentId++;

				if (sendingReliable)
				{
					LastReliableSentId = LastSentId;
					LastReliableStatus = PacketStatus.Unknown;
				}

				header1 = (LastSentId & ~((uint)1 << 31)) | ((sendingReliable ? (uint)1 << 31 : 0));
				header2 = (LastReceivedId & ~((uint)1 << 31)) | (RemoteReliableSequence ? (uint)1 << 31 : 0);
				sendAck = AckNeeded;
				AckNeeded = false;
			}

			int headerSize = 8 + WriteHeaderAdditionalSize;

			if (sending == null)
			{
				sending = NonReliablePackage;
				if (!sendAck && NonReliablePackage.Commands.Count == 0)
					return null;

				Monitor.Enter(NonReliablePackage);
			}
			else if (!sendAck && sending.Commands.Count == 0)
				return null;

			try
			{
				var nextPackage = new WriteRawData();
				nextPackage.WriteInt((int)header1);
				nextPackage.WriteInt((int)header2);

				WriteAdditionalHeader(nextPackage);

				sending.WriteTo(nextPackage);

				if (sending == NonReliablePackage)
					NonReliablePackage.Clear();

				return nextPackage;
			}
			finally
			{
				if (sending == NonReliablePackage)
					Monitor.Exit(NonReliablePackage);
			}
		}

		public int WriteHeaderAdditionalSize
		{
			get
			{
				if (LocalType == ChanType.LocalServer)
					return 0;
				return Protocol == Protocol.R1Q2 ? 1 : 2;
			}
		}

		public void WriteAdditionalHeader(WriteRawData newPackage)
		{
			if (LocalType == ChanType.LocalClient)
			{
				if (Protocol == Protocol.R1Q2)
					newPackage.WriteByte((byte)QPort);
				else
					newPackage.WriteShort((short)QPort);
			}
		}
	}
}
