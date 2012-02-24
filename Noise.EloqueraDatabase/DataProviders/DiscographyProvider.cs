using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class DbDiscographyProvider : BaseDataProvider<DbDiscographyRelease>, IDiscographyProvider {
		public DbDiscographyProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddDiscography( DbDiscographyRelease release ) {
			Condition.Requires( release ).IsNotNull();

			InsertItem( release );
		}

		public void RemoveDiscography( DbDiscographyRelease release ) {
			Condition.Requires( release ).IsNotNull();

			DeleteItem( release );
		}

		public IDataProviderList<DbDiscographyRelease> GetDiscography( long artistId ) {
			return( TryGetList( "SELECT DbDiscographyRelease WHERE Artist = @artistId", new Dictionary<string, object> {{ "artistId", artistId }}, "Exception - GetDiscography" ));
		}
	}
}
