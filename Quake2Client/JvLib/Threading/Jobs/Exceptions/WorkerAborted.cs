namespace Jv.Threading.Jobs
{
    public class WorkerAborted : InvalidWorkerState
    {
        public WorkerAborted(IWorker worker, System.Exception reason)
            : this(worker, "Worker was aborted", reason) { }

        public WorkerAborted(IWorker worker, string message, System.Exception reason)
            : base(worker, WorkerState.Aborted, message)
        {
            Reason = reason;
        }

        public System.Exception Reason { get; private set; }
    }
}
