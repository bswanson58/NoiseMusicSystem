using System.Collections.Generic;

namespace Noise.Infrastructure.Dto {
	public class LyricsInfo {
		public DbLyric					MatchedLyric { get; private set; }

		private readonly List<DbLyric>	mPossibleLyrics;

		public LyricsInfo( DbLyric match, IEnumerable<DbLyric> otherLyrics ) {
			MatchedLyric = match;
			mPossibleLyrics = new List<DbLyric>( otherLyrics );
		}

		public IEnumerable<DbLyric> PossibleLyrics {
			get{ return( mPossibleLyrics ); }
		}

		public bool HasMatchedLyric {
			get{ return( MatchedLyric != null ); }
		}

		public void SetMatchingLyric( DbLyric match ) {
			MatchedLyric = match;

			if(!mPossibleLyrics.Exists( lyric => lyric.DbId == match.DbId )) {
				mPossibleLyrics.Add( match );
			}
		}
	}
}
