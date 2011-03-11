using Jv.Threading.Collections.Generic;
using System;

namespace Jv.Threading.Jobs
{
	public class Pool<Type> : IWorker<Type>
	{
		#region Fields
		readonly Worker[] _workers;
		readonly SyncQueue<IJob> _jobs;
		readonly ParameterMethod<Type> _method;
		public WorkerState State { get; private set; }
		Exception _abortException;
		#endregion

		#region Constructors
        public Pool(ParameterMethod<Type> method)
            : this((uint)Environment.ProcessorCount, method) { }

		public Pool(uint workers, ParameterMethod<Type> method)
		{
			if (workers == 0)
				throw new Exception("Thread Pool must contain at least one Worker");

			_method = method;
			_jobs = new SyncQueue<IJob>();
			_workers = new Worker[workers];

			for (uint i = 0; i < workers; i++)
				_workers[i] = new Worker(_jobs);
		}
		#endregion

		public void Execute(Type obj)
		{
			lock (this)
			{
				if (State == WorkerState.ExitScheduled)
					throw new InvalidWorkerState(this, State, "Exit was scheduled");

				if (State == WorkerState.Exited)
					throw new InvalidWorkerState(this, State, "Worker already exited");

				if (State == WorkerState.Aborted)
					throw new WorkerAborted(this, _abortException);
			}

			_jobs.Add(new Job<Type>(obj, _method));
		}

		public void Exit()
		{
			lock (this)
			{
				if (State == WorkerState.Exited)
					return;

				if (State == WorkerState.Aborted)
					throw _abortException;

				if (State != WorkerState.ExitScheduled)
				{
					for (uint i = 0; i < _workers.Length; i++)
						_jobs.Add(Worker.ExitJobs);

					State = WorkerState.ExitScheduled;
				}
			}
		}

		public void ExitAndWait()
		{
			Exit();
			Wait(true);
			
			lock(this)
				State = WorkerState.Exited;
		}

		public void Wait()
		{
			Wait(false);
		}

		public void Wait(bool checkState)
		{
			foreach (Worker worker in _workers)
				worker.Wait(checkState);
		}
		
		public void Abort(Exception reason)
		{
			lock(this)
			{
				if (State == WorkerState.Exited || State == WorkerState.Aborted)
					return;

				State = WorkerState.Aborted;
				_abortException = reason;

				bool abortCurrentWorker = false;

				foreach (Worker worker in _workers)
				{
					if (worker != Worker.CurrentWorker)
						worker.Abort(reason);
					else abortCurrentWorker = true;
				}

				if (abortCurrentWorker)
					Worker.CurrentWorker.Abort(reason);
			}
		}
	}
}
