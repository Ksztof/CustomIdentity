using CustomIdentity.Domain;
using Microsoft.Extensions.Configuration;

namespace CustomIdentity.Core.Repositories
{
	public abstract class Repository
	{
		private CustomIdentityDb _db = null;
		public Repository()
		{
		}

		public virtual CustomIdentityDb Db
		{
			get
			{
				if (_db == null)
				{
					_db = new CustomIdentityDb();
				}

				return _db;
			}
		}
	}
}
