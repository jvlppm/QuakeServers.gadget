using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Jv.Networking
{
	public class ReadRawData
	{
		public int CurrentPosition { get; set; }

		public byte[] Data { get; protected set; }

		public ReadRawData(byte[] data)
		{
			CurrentPosition = 0;
			Data = data;
		}

		public bool EndOfData { get { return CurrentPosition >= Data.Length; } }

		#region Read Data
		public byte ReadByte()
		{
			return Data[CurrentPosition++];
		}

		public short ReadShort()
		{
			return (short)((ReadByte()) + (((uint)ReadByte()) << 8));
		}

		public int ReadInt()
		{
			return (int)((ReadByte()) + (((uint)ReadByte()) << 8) + (((uint)ReadByte()) << 16) + (((uint)ReadByte()) << 24));
		}

		public string ReadString()
		{
			return ReadString('\0');
		}

		public string ReadString(int length)
		{
			StringBuilder text = new StringBuilder(length);

			for(int i = 0; i < length && CurrentPosition < Data.Length; i++)
				text.Append(ReadChar());

			return text.ToString();
		}

		public string ReadString(params char[] endChars)
		{
			StringBuilder text = new StringBuilder();

			while (CurrentPosition < Data.Length)
			{
				char ch = ReadChar();
				if (endChars.Contains(ch))
					break;

				text.Append(ch);
			}

			return text.ToString();
		}

		public char ReadChar()
		{
			return (char)ReadByte();
		}

		public byte[] ReadBytes(int count)
		{
			byte[] data = new byte[count];
			for(int i = 0; i < count; i++)
				data[i] = ReadByte();

			return data;
		}
		#endregion
	}

	public class WriteRawData
	{
		public int CurrentPosition { get { return Data.Count; } }

		public List<byte> Data { get; protected set; }

		public WriteRawData()
		{
			Data = new List<byte>();
		}

		#region WriteData
		public void WriteByte(byte value)
		{
			Data.Add(value);
		}
		public void WriteShort(short value)
		{
			WriteByte((byte)(value & 0xff));
			WriteByte((byte)(value >> 8));
		}
		public void WriteInt(int value)
		{
			WriteByte((byte)(value & 0xff));
			WriteByte((byte)((value >> 8) & 0xff));
			WriteByte((byte)((value >> 16) & 0xff));
			WriteByte((byte)(value >> 24));
		}
		public void WriteString(string text)
		{
			foreach (char ch in text)
				WriteByte((byte)ch);
			WriteByte(0x00);
		}
		public void WriteBytes(byte[] bytes)
		{
			Data.AddRange(bytes);
		}
		#endregion
	}
}
