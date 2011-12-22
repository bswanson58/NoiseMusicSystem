using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class PlayListProvider : BaseDataProvider<DbPlayList>, IPlayListProvider {
		public PlayListProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DataProviderList<DbPlayList> GetPlayLists() {
			return( TryGetList( "SELECT DbPlayList", "GetPlayLists" ));
		}
	}
}
