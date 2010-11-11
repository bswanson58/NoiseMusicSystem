using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public enum DbItemChanged {
		Insert,
		Update,
		Delete,
		Favorite,
		Rating
	}

	public class DbItemChangedArgs {
		private	readonly DbBase	mItem;
		public	long			ItemId { get; private set; }
		public	DbItemChanged	Change { get; private set; }

		public DbItemChangedArgs( long itemId, DbItemChanged change ) {
			ItemId = itemId;
			Change = change;
		}

		public DbItemChangedArgs( DbBase item, DbItemChanged change ) {
			mItem = item;
			ItemId = item.DbId;
			Change = change;
		}

		public DbBase GetItem( IDataProvider dataProvider ) {
			var retValue = mItem;

			if(( mItem == null ) &&
			   ( dataProvider != null )) {
				retValue = dataProvider.GetItem( ItemId );
			}

			return( retValue );
		}
	}
}
