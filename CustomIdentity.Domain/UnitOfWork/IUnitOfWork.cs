namespace CustomIdentity.Domain.UnitOfWork
{
	public interface IUnitOfWork
	{
		Task BeginTransactionAsync();
		Task CommitAsync();
		Task RollbackAsync();
	}
}
