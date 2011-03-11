using System;
using System.Threading;
using Jv.Threading.Collections.Generic;
using System.Collections.Generic;

namespace Jv.Threading.Jobs
{
	public class Worker : IWorker<IJob>
	{
		#region Static Fields
		static uint _nextId = 1;
		static readonly Dictionary<Thread, Worker> Workers = new Dictionary<Thread, Worker>();
        internal static readonly IJob ExitJobs = new Job();

		public static Worker CurrentWorker
		{
			get { return Workers.ContainsKey(Thread.CurrentThread) ? Workers[Thread.CurrentThread] : null; }
		}
		#endregion

		#region Fields
        public uint Id { get; private set; }
        public WorkerState State { get; private set; }

		readonly Thread _thread;
		readonly SyncQueue<IJob> _jobs;
		readonly Queue<IJob> _beforeExit;

		public Worker Parent { get; private set; }

        Exception _abortException;
		#endregion

		#region Constructors
		public Worker() : this(new SyncQueue<IJob>(), string.Format("Worker {0}", _nextId)) { }

		public Worker(string name) : this(new SyncQueue<IJob>(), name) { }

		public Worker(SyncQueue<IJob> jobs) : this(jobs, string.Format("Worker {0}", _nextId)) { }

		public Worker(SyncQueue<IJob> jobs, string name)
		{
			_beforeExit = new Queue<IJob>();
			_jobs = jobs;
			_thread = new Thread(ExecuteJobs) { Name = name };
			_thread.Start();
			State = WorkerState.Alive;

			Id = _nextId++;
			Workers.Add(_thread, this);
			Parent = CurrentWorker;
		}
		#endregion

		#region Public Methods
		public void Execute(IJob job)
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

			_jobs.Add(job);
		}

		public void Execute(ThreadStart method)
		{
			Execute(new Job(method));
		}

		public void Execute<ParameterType>(ParameterType parameter, ParameterMethod<ParameterType> method)
		{
			Execute(new Job<ParameterType>(parameter, method));
		}

		public void Exit()
		{
			lock (this)
			{
				if (State != WorkerState.Alive)
					return;

				State = WorkerState.ExitScheduled;
				_jobs.Add(ExitJobs);
			}
		}

		public void Abort(Exception reason)
		{
			lock (this)
			{
				if (State == WorkerState.Aborted || State == WorkerState.Exited)
					return;

				_abortException = reason;

				State = WorkerState.Aborted;
				_thread.Abort();
			}
		}

		public void ExecuteBeforeExit(IJob job)
		{
			_beforeExit.Enqueue(job);
		}

        public void ExitAndWait()
        {
            Exit();
            Wait(true);
        }

		public void Wait()
		{
			Wait(false);
		}

        public void Wait(bool checkState)
        {
            _thread.Join();

			if (checkState && State == WorkerState.Aborted)
				throw _abortException;
        }
		#endregion

		#region Private Methods
		void ExecuteJobs()
		{
			IJob nextJob = _jobs.RemoveNext();
			while (nextJob != ExitJobs)
			{
				if (nextJob != null)
					nextJob.Execute();
				nextJob = _jobs.RemoveNext();
			}

			State = WorkerState.Exited;

			while (_beforeExit.Count > 0)
			{
				nextJob = _beforeExit.Dequeue();
				if (nextJob != null)
					nextJob.Execute();
			}
		}
		#endregion
	}
}