﻿using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public class EfUpdateShell<TEntity> : EfProviderShell, IDataUpdateShell<TEntity> where TEntity : DbBase {
		public	TEntity	Item { get; private set; }

		public EfUpdateShell( IDbContext context, TEntity item )
			: base( context ) {
			Item = item;
		}

		public void Update() {
			mContext.SaveChanges();
		}
	}
}
