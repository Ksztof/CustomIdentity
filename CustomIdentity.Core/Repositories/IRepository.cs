using CustomIdentity.Core.HelperModels;

namespace CustomIdentity.Core.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<ActionResultM<T>> AddAsync(T entity);
		Task<ActionResultM<T>> UpdateAsync(T entity);
		Task<ActionResultM<T>> DeleteAsync(T entity);
		Task<ActionResultM<T>> GetByIdAsync(int id);
		Task<ActionResultM<IList<T>>> GetListAsync();
	}
}
