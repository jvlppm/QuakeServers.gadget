using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace Jv.Threading.Collections.Generic
{
	/*public class SyncStack<Type> : IEnumerable<Type>, ISyncCollection<Type>
	{
		#region Fields
		readonly Stack<Type> _items;
		readonly Semaphore _counter;
		#endregion

		#region Constructors
		public SyncStack()
		{
			_items = new Stack<Type>();
			_counter = new Semaphore(0, int.MaxValue);
		}
		#endregion

		#region Implementation of IEnumerable
		public IEnumerator<Type> GetEnumerator()
		{
			return _items.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Data Access
		public void Push(Type obj)
		{
			lock (_items)
				_items.Push(obj);

			_counter.Release();
		}

		public Type Pop()
		{
			_counter.WaitOne();

			lock (_items)
				return _items.Pop();
		}
		#endregion

		#region ISyncCollection<Type> Members

		public void Add(Type obj)
		{
			Push(obj);
		}

		public Type RemoveNext()
		{
			return Pop();
		}

		#endregion
	}*/
}
