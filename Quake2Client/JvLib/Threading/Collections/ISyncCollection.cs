namespace Jv.Threading.Collections
{
	public interface ISyncCollection
	{
		void Add(object obj);
		object RemoveNext();
	}
}
