using System;

namespace Jv.Threading.Jobs
{
	public interface IWorker
	{
		void Wait();
		void Wait(bool checkState);
		void Exit();
		void Abort(Exception reason);
	}

	public interface IWorker<JobType> : IWorker
	{
		void Execute(JobType job);
	}
}