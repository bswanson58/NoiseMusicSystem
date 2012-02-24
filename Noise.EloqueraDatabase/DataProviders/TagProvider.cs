using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class TagProvider : BaseDataProvider<DbTag>, ITagProvider {
		public TagProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddTag( DbTag tag ) {
			InsertItem( tag );
		}

		public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			return( TryGetList( "SELECT DbTag Where TagGroup = @group", new Dictionary<string, object> {{ "group", forGroup }}, "Exception - GetTagList" ));
		}
	}
}
