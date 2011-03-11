using System;

namespace Jv.Threading.Jobs
{
    public class InvalidWorkerState : Exception
    {
        public InvalidWorkerState(IWorker worker, WorkerState state, string message)
            : base(message)
        {
            Worker = worker;
            State = state;
        }

        public IWorker Worker { get; private set; }
        public WorkerState State { get; private set; }
    }
}
