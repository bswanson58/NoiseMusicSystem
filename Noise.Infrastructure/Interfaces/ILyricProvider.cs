using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILyricProvider {
		void						AddLyric( DbLyric lyric );

		IDataProviderList<DbLyric>	GetPossibleLyrics( DbArtist artist, DbTrack track );
		IDataProviderList<DbLyric>	GetLyricsForArtist( DbArtist artist );
		
		IDataUpdateShell<DbLyric>	GetLyricForUpdate( long lyricId );
	}
}
