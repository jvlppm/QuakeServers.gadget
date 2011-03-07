using System.Runtime.InteropServices;

namespace Wrapper
{
	[ComVisible(true)]
	public class Quake2Client
	{
		public Quake2Client()
		{
			Id = "Quake2";
		}

		public string Id { get; private set; }
	}
}
