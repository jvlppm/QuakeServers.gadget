using System.Threading;
using Jv.Threading.Jobs;

namespace Jv.Threading
{
	public static class Parallel
	{
		public static void Start(ThreadStart method)
		{
			Worker worker = new Worker();
			worker.Execute(method);
			worker.Exit();
		}

		public static void Start<ParameterType>(ParameterType parameter, ParameterMethod<ParameterType> method)
		{
			Worker worker = new Worker();
			worker.Execute(parameter, method);
			worker.Exit();
		}

		public static void Start(string name, ThreadStart method)
		{
			Worker worker = new Worker(name);
			worker.Execute(method);
			worker.Exit();
		}

		public static void Start<ParameterType>(string name, ParameterType parameter, ParameterMethod<ParameterType> method)
		{
			Worker worker = new Worker(name);
			worker.Execute(parameter, method);
			worker.Exit();
		}
	}
}