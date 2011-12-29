using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayListProvider {
		void							AddPlayList( DbPlayList playList );
		void							DeletePlayList( DbPlayList playList );

		DbPlayList						GetPlayList( long playListId );

		DataProviderList<DbPlayList>	GetPlayLists();

		DataUpdateShell<DbPlayList>		GetPlayListForUpdate( long playListId );
	}
}
