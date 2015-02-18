using System.Collections.Generic;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyArtist : PlayExhaustedStrategyRandomBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private	readonly List<long>			mAlbums;
		private long						mArtistId;
		private string						mArtistName;

		public PlayExhaustedStrategyArtist( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ILogPlayStrategy log ) :
			base( ePlayExhaustedStrategy.PlayArtist, "Play Artist...", "Play random tracks from the selected artist.", "Artist", albumProvider, trackProvider, log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;

			mAlbums = new List<long>();
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from {0}", mArtistName ));
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( Parameters is PlayStrategyParameterDbId ) {
				var	dbParam = Parameters as PlayStrategyParameterDbId;

				if( mArtistId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mArtistId = dbParam.DbItemId;

					using( var albumList = mAlbumProvider.GetAlbumList( mArtistId )) {
						mAlbums.AddRange( from album in albumList.List select album.DbId );
					}

					var artist = mArtistProvider.GetArtist( mArtistId );
					if( artist != null ) {
						mArtistName = artist.Name;
					}
				}
			}
		}

		protected override DbTrack SelectATrack() {
			var albumId = mAlbums.Skip( NextRandom( mAlbums.Count )).FirstOrDefault();
			var album = mAlbumProvider.GetAlbum( albumId );

			return( RandomTrackFromAlbum( album ));
		}
	}
}
