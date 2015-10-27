using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class AlbumArtworkProvider : IAlbumArtworkProvider {
		private readonly IAlbumProvider	mAlbumProvider;
		private readonly Random			mRandom;

		public AlbumArtworkProvider( IAlbumProvider albumProvider ) {
			mAlbumProvider = albumProvider;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public Artwork GetAlbumCover( DbAlbum forAlbum ) {
				var albumInfo = mAlbumProvider.GetAlbumSupportInfo( forAlbum.DbId );
				Artwork	cover = null;

				if(( albumInfo.AlbumCovers != null ) &&
				   ( albumInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					cover = (( from Artwork artwork in albumInfo.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in albumInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								albumInfo.AlbumCovers[0];
				}

				if(( cover == null ) &&
				   ( albumInfo.Artwork != null ) &&
				   ( albumInfo.Artwork.GetLength( 0 ) > 0 )) {
					cover = ( from Artwork artwork in albumInfo.Artwork
							  where artwork.Name.IndexOf( "front", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault();

					if(( cover == null ) &&
					   ( albumInfo.Artwork.GetLength( 0 ) == 1 )) {
						cover = albumInfo.Artwork[0];
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
	}
}
