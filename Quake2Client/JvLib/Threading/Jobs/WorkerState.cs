namespace Jv.Threading.Jobs
{
	public enum WorkerState
	{
		Alive,
		ExitScheduled,
		Exited,
		Aborted
	}
}