using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class ArtistArtworkProvider : IArtistArtworkProvider {
		private readonly IMetadataManager		mMetadataManager;
		private readonly IStorageFolderSupport	mStorageSupport;
		private readonly Random					mRandom;

		public ArtistArtworkProvider( IMetadataManager metadataManager, IStorageFolderSupport storageSupport ) {
			mMetadataManager = metadataManager;
			mStorageSupport = storageSupport;

			mRandom = new Random( DateTime.Now.Millisecond );
		}

		public Artwork GetDefaultArtwork( DbArtist forArtist ) {
			return( mMetadataManager.GetArtistArtwork( forArtist.Name ));
		}

		public Artwork GetRandomArtwork( DbArtist forArtist ) {
			var retValue = GetDefaultArtwork( forArtist );
			var otherImages = ArtistImageList( forArtist );

			if( otherImages.Any()) {
				var imageIndex = mRandom.Next( 0, otherImages.Count());

				if( imageIndex > 0 ) {
					var imageName = otherImages.Skip( imageIndex - 1 ).Take( 1 ).FirstOrDefault();

					if( !string.IsNullOrWhiteSpace( imageName ) ) {
						retValue = new Artwork( new DbArtwork( forArtist.DbId, ContentType.ArtistPrimaryImage )) { Name = imageName };

						LoadArtwork( imageName, retValue );
					}
				}
			}

			return( retValue );
		}

		public int ImageCount( DbArtist forArtist ) {
			return( 1 + ArtistImageList( forArtist ).Count );
		}

		private List<string> ArtistImageList( DbArtist forArtist ) {
			var retValue = new List<string>();
			var artworkPath = Path.Combine( mStorageSupport.GetArtistPath( forArtist.DbId ), Constants.LibraryMetadataFolder );

			if( Directory.Exists( artworkPath )) {
				var files = Directory.GetFiles( artworkPath );

				retValue.AddRange( from file in files where mStorageSupport.DetermineFileType( file ) == eFileType.Picture select file );
			}

			return( retValue );
		} 

		private void LoadArtwork( string fileName, Artwork artwork ) {
			if( File.Exists( fileName ) ) {
				using( var file = new FileStream( fileName, FileMode.Open, FileAccess.Read )) {
					artwork.Image = new byte[file.Length];

					file.Read( artwork.Image, 0, artwork.Image.Length );
				}
			}
		}
	}
}
