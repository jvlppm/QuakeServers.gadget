using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace Jv.Threading.Collections.Generic
{
	public class SyncQueue<Type> : IEnumerable<Type>, ISyncCollection<Type>
	{
		#region Fields
		readonly Queue<Type> _items;
		readonly Semaphore _counter, _waitingFlush;
		#endregion

		#region Constructors
		public SyncQueue()
		{
			_items = new Queue<Type>();
			_counter = new Semaphore(0, int.MaxValue);
			_waitingFlush = new Semaphore(0, int.MaxValue);
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
		public void Enqueue(Type obj)
		{
			lock (_items)
				_items.Enqueue(obj);

			_counter.Release();
		}

		public Type Dequeue()
		{
			do
			{
				_counter.WaitOne();
				lock (_items)
				{
					if (_items.Count != 0)
						return _items.Dequeue();
					else _waitingFlush.Release();
				}
			} while (true);
		}
		#endregion

		#region ISyncCollection<Type> Members

		public void Add(Type obj)
		{
			Enqueue(obj);
		}

		public Type RemoveNext()
		{
			return Dequeue();
		}

		public void Flush()
		{
			_counter.Release();
			_waitingFlush.WaitOne();
		}

		#endregion
	}
}
