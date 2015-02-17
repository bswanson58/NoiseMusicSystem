﻿using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class EfUpdateShell<TEntity> : EfProviderShell, IDataUpdateShell<TEntity> where TEntity : DbBase {
		public	TEntity	Item { get; private set; }

		public EfUpdateShell( IDbContext context, TEntity item )
			: base( context ) {
			Item = item;
		}

		public virtual void Update() {
			var versionable = Item as IVersionable;

			if( versionable != null ) {
				versionable.UpdateVersion();
			}

			mContext.SaveChanges();
		}
	}
}
