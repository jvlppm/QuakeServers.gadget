using System.Collections;
using System.Threading;

namespace Jv.Threading.Collections
{
	public class SyncQueue : IEnumerable, ISyncCollection
	{
		#region Attributes
		readonly Queue _items;
		readonly Semaphore _counter;
		#endregion

		#region Constructors
		public SyncQueue()
		{
			_items = new Queue();
			_counter = new Semaphore(0, int.MaxValue);
		}
		#endregion

		#region Data Access
		/// <summary>
		/// Adds an object to the end of the Jv.Threading.SyncQueue and releases any thread waiting in Dequeue.
		/// </summary>
		/// <param name="obj"></param>
		public void Enqueue(object obj)
		{
			lock (_items.SyncRoot)
				_items.Enqueue(obj);

			_counter.Release();
		}

		/// <summary>
		/// Waits for a item and returns the object at the beginning of the Jv.Threading.SyncQueue.
		/// </summary>
		/// <returns></returns>
		public object Dequeue()
		{
			_counter.WaitOne();

			lock (_items.SyncRoot)
				return _items.Dequeue();
		}
		#endregion

		#region Implementation of IEnumerable
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///	An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}
		#endregion

		#region ISyncCollection Members

		public void Add(object obj)
		{
			Enqueue(obj);
		}

		public object RemoveNext()
		{
			return Dequeue();
		}

		#endregion
	}
}
