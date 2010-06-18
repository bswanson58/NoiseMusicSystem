﻿namespace Noise.Core.MetaData {
	public class DbTopItems : ExpiringContent {
		public	long		AssociatedItem { get; private set; }
		public	string[]	TopItems { get; set; }

		public DbTopItems( long associatedItem ) {
			AssociatedItem = associatedItem;
		}
	}
}
