namespace Quake2
{
	public interface IAbortable
	{
		bool Abort { get; set; }
	}

	public interface IHandleable
	{
		bool Handled { get; set; }
	}
}
