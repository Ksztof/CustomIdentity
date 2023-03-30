using Microsoft.EntityFrameworkCore.Storage;

namespace CustomIdentity.Domain.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly CustomIdentityDb _dbContext;
		private IDbContextTransaction _transaction;

		public UnitOfWork(CustomIdentityDb dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task BeginTransactionAsync()
		{
			_transaction = await _dbContext.Database.BeginTransactionAsync();
		}

		public async Task CommitAsync()
		{
			await _transaction.CommitAsync();
		}

		public async Task RollbackAsync()
		{
			await _transaction.RollbackAsync();
		}
	}
}
