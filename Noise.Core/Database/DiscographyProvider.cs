using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class DbDiscographyProvider : BaseDataProvider<DbDiscographyRelease>, IDiscographyProvider {
		public DbDiscographyProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddDiscography( DbDiscographyRelease release ) {
			Condition.Requires( release ).IsNotNull();

			InsertItem( release );
		}

		public void RemoveDiscography( DbDiscographyRelease release ) {
			Condition.Requires( release ).IsNotNull();

			DeleteItem( release );
		}

		public DataProviderList<DbDiscographyRelease> GetDiscography( long artistId ) {
			return( TryGetList( "SELECT DbDiscographyRelease WHERE Artist = @artistId", new Dictionary<string, object> {{ "artistId", artistId }}, "Exception - GetDiscography" ));
		}
	}
}
