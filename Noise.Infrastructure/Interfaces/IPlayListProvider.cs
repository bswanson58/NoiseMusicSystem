using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayListProvider {
		void							AddPlayList( DbPlayList playList );
		void							DeletePlayList( DbPlayList playList );

		DbPlayList						GetPlayList( long playListId );

		IDataProviderList<DbPlayList>	GetPlayLists();

		IDataUpdateShell<DbPlayList>	GetPlayListForUpdate( long playListId );
	}
}
