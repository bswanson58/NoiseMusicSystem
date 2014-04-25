using System;
using System.Data.Entity;

namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IDbContext : IDisposable {
		IDbSet<TEntity>	Set<TEntity>() where TEntity : class;

		int				SaveChanges();

		Database		Database { get; }
		bool			IsValidContext { get; }
	}
}
