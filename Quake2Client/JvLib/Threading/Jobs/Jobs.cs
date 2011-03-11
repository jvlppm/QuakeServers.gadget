using System.Threading;

namespace Jv.Threading.Jobs
{
	public interface IJob
	{
		void Execute();
	}

	public class Job : IJob
	{
		#region Constructors
		internal Job() { }
		public Job(ThreadStart method)
		{
			Method = method;
		}

		public static implicit operator Job(ThreadStart method)
		{
			return new Job(method);
		}
		#endregion

		ThreadStart Method { get; set; }

		public void Execute()
		{
			if(Method != null)
				Method();
		}
	}

	public class Job<ParameterType> : IJob
	{
		#region Constructors
		public Job(ParameterType parameter, ParameterMethod<ParameterType> method)
		{
			Method = method;
			Parameter = parameter;
		}
		#endregion

		ParameterType Parameter { get; set; }
		ParameterMethod<ParameterType> Method { get; set; }

		public void Execute()
		{
			if (Method != null)
				Method(Parameter);
		}
	}
}