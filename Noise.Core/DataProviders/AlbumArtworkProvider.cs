using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class AlbumArtworkProvider : IAlbumArtworkProvider {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly IArtworkProvider	mArtworkProvider;
		private readonly INoiseLog			mLog;
		private readonly Random				mRandom;

		public AlbumArtworkProvider( IAlbumProvider albumProvider, IArtworkProvider artworkProvider, INoiseLog log ) {
			mAlbumProvider = albumProvider;
			mArtworkProvider = artworkProvider;
			mLog = log;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public Artwork GetAlbumCover( DbAlbum forAlbum ) {
			Artwork	cover = null;

			if( forAlbum != null ) {
				var albumInfo = mAlbumProvider.GetAlbumSupportInfo( forAlbum.DbId );

				if(( albumInfo.AlbumCovers != null ) &&
				   ( albumInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					cover = ( from Artwork artwork in albumInfo.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault() ??
							albumInfo.AlbumCovers[0];
				}

				if(( cover == null ) &&
				   ( albumInfo.Artwork != null ) &&
				   ( albumInfo.Artwork.GetLength( 0 ) > 0 )) {
					cover = ( from Artwork artwork in albumInfo.Artwork
							  where artwork.Name.IndexOf( "front", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault() ??
							( from Artwork artwork in albumInfo.Artwork
							  where artwork.Name.IndexOf( "cover", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault() ??
							( from Artwork artwork in albumInfo.Artwork
							  where artwork.Name.IndexOf( "folder", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault() ??
							albumInfo.Artwork[0];
				}
			}

			return( cover );
		}

		public Artwork GetNextAlbumArtwork( DbAlbum forAlbum, int index ) {
			Artwork	retValue = GetAlbumCover( forAlbum );
			var albumInfo = mAlbumProvider.GetAlbumSupportInfo( forAlbum.DbId );

			if(( albumInfo.Artwork != null ) &&
			   ( albumInfo.Artwork.GetLength( 0 ) > 0 )) {
				index = index % ( albumInfo.Artwork.GetLength( 0 ) + 1 );

				if( index > 0 ) {
					retValue = albumInfo.Artwork[index - 1];
				}
			}

			return( retValue );
		}

		public Artwork GetRandomAlbumArtwork( DbAlbum forAlbum ) {
			Artwork	retValue = GetAlbumCover( forAlbum );
			var albumInfo = mAlbumProvider.GetAlbumSupportInfo( forAlbum.DbId );

			if(( albumInfo.Artwork != null ) &&
			   ( albumInfo.Artwork.GetLength( 0 ) > 1 )) {
				retValue = albumInfo.Artwork[mRandom.Next(albumInfo.Artwork.GetLength( 0 ) - 1 )];
			}

			return( retValue );
		}

		public int ImageCount( DbAlbum forAlbum ) {
			var albumInfo = mAlbumProvider.GetAlbumSupportInfo( forAlbum.DbId );

			return( albumInfo.Artwork.GetLength( 0 ));
		}

		public void SetAlbumCover( long albumId, long artworkId ) {
			try {
				var artworkList = mArtworkProvider.GetAlbumArtwork( albumId );
				foreach( var artwork in artworkList ) {
					if(( artwork.IsUserSelection ) &&
					   ( artwork.DbId != artworkId )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = false;

								artworkUpdater.Update();
							}
						}
					}

					if(( artwork.DbId == artworkId ) &&
					   (!artwork.IsUserSelection )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = true;

								artworkUpdater.Update();
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting album cover {0} for Album {1}", artworkId, albumId ), ex );
			}
		}
	}
}
