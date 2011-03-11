using Jv.Networking;

namespace Quake2.Network.Commands
{
	public interface ICommand
	{
		int Size();
		void WriteTo(WriteRawData package);
	}
}