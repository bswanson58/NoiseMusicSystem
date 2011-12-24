using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILyricProvider {
		void						AddLyric( DbLyric lyric );

		DataProviderList<DbLyric>	GetPossibleLyrics( DbArtist artist, DbTrack track );
		DataUpdateShell<DbLyric>	GetLyricForUpdate( long lyricId );
	}
}
