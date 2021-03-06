﻿using System.Collections.Generic;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class EfProviderList<T> : EfProviderShell, IDataProviderList<T> {
		public		IEnumerable<T>	List { get; private set; }

		public EfProviderList( IDbContext context, IEnumerable<T> list ) :
			base( context ) {
			mContext = context;
			List = list;
		}
	}
}
