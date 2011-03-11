namespace Jv.Threading.Collections.Generic
{
	public interface ISyncCollection<Type>
	{
		void Add(Type obj);
		Type RemoveNext();
		void Flush();
	}
}
