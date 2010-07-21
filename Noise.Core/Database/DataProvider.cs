using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider {
		private readonly object				mLockObject;
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;

		public DataProvider( IUnityContainer container ) {
			mLockObject = new object();

			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public long GetObjectIdentifier( object dbObject ) {
			return( mDatabase.Database.GetUid( dbObject ));
		}

		public IEnumerable<DbArtist> GetArtistList() {
			return( from DbArtist artist in mDatabase.Database select artist );
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			var parms = mDatabase.Database.CreateParameters();

			parms["artistId"] = album.Artist;

			return( mDatabase.Database.ExecuteScalar( "SELECT DbArtist WHERE $ID = @artistId", parms ) as DbArtist );
		}

		public IEnumerable<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			var artistId = mDatabase.Database.GetUid( forArtist );

			return( from DbAlbum album in mDatabase.Database where album.Artist == artistId select album );
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			var parms = mDatabase.Database.CreateParameters();

			parms["albumId"] = track.Album;

			return( mDatabase.Database.ExecuteScalar( "SELECT DbAlbum WHERE $ID = @albumId", parms ) as DbAlbum );
		}

		public IEnumerable<DbTrack> GetTrackList( DbAlbum forAlbum ) {
			var albumId = mDatabase.Database.GetUid( forAlbum );

			return( from DbTrack track in mDatabase.Database where track.Album == albumId select track );
		}

		public IEnumerable<DbTrack> GetTrackList( DbArtist forArtist ) {
			var	retValue = new List<DbTrack>();
			var artistId = mDatabase.Database.GetUid( forArtist );
			var albumList = from DbAlbum album in mDatabase.Database where album.Artist == artistId select album;

			foreach( DbAlbum album in albumList ) {
				var albumId = mDatabase.Database.GetUid( album );
				var trackList = from DbTrack track in mDatabase.Database where track.Album == albumId select track;

				retValue.AddRange( trackList );
			}

			return( retValue );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			var trackId = mDatabase.Database.GetUid( forTrack );

			Condition.Requires( trackId ).IsNotLessOrEqual( 0 );

			return(( from StorageFile file in mDatabase.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault());
		}

		public object GetMetaData( StorageFile forFile ) {
			var parm = mDatabase.Database.CreateParameters();

			parm["id"] = forFile.MetaDataPointer;

			return( mDatabase.Database.ExecuteScalar( "SELECT data WHERE $ID = @id", parm ));
		}

		public ArtistSupportInfo GetArtistSupportInfo( DbArtist forArtist ) {
			ArtistSupportInfo	retValue;

			lock( mLockObject ) {
				var artistId = mDatabase.Database.GetUid( forArtist );
				var parms = mDatabase.Database.CreateParameters();

				parms["artistId"] = artistId;
				parms["type"] = ArtworkTypes.ArtistImage;

				retValue = new ArtistSupportInfo(( from DbTextInfo bio in mDatabase.Database where bio.AssociatedItem == artistId && bio.InfoType == TextInfoTypes.Biography select bio ).FirstOrDefault(),
//												 ( from DbArtwork artwork in mDatabase.Database where artwork.AssociatedItem == artistId && artwork.ArtworkType == ArtworkTypes.ArtistImage select artwork ).FirstOrDefault(),
												   mDatabase.Database.ExecuteScalar( "SELECT DbArtwork Where AssociatedItem = @artistId AND ArtworkType = @type", parms ) as DbArtwork,
												 ( from DbSimilarItems items in mDatabase.Database where items.AssociatedItem == artistId select items ).FirstOrDefault(),
												 ( from DbTopItems items in mDatabase.Database where  items.AssociatedItem == artistId select  items ).FirstOrDefault());
			}

			return( retValue );
		}

		public AlbumSupportInfo GetAlbumSupportInfo( DbAlbum forAlbum ) {
			AlbumSupportInfo	retValue = null;

			lock( mLockObject ) {
				var albumId = mDatabase.Database.GetUid( forAlbum );
				var albumTrack = ( from DbTrack track in mDatabase.Database where track.Album == albumId select track ).FirstOrDefault();

				if( albumTrack != null ) {
					var trackId = mDatabase.Database.GetUid( albumTrack );
					var	fileTrack = ( from StorageFile file in mDatabase.Database where file.MetaDataPointer == trackId select file ).FirstOrDefault();

					if( fileTrack != null ) {
						var parms = mDatabase.Database.CreateParameters();

						parms["folderId"] = fileTrack.ParentFolder;
						parms["coverType"] = ArtworkTypes.AlbumCover;
						parms["otherType"] = ArtworkTypes.AlbumOther;

						retValue = new AlbumSupportInfo( mDatabase.Database.ExecuteQuery( "SELECT DbArtwork WHERE FolderLocation = @folderId AND ArtworkType = @coverType", parms ).OfType<DbArtwork>().ToArray(),
														 mDatabase.Database.ExecuteQuery( "SELECT DbArtwork WHERE FolderLocation = @folderId AND ArtworkType = @otherType", parms ).OfType<DbArtwork>().ToArray(),
														 ( from DbTextInfo info in mDatabase.Database where info.FolderLocation == fileTrack.ParentFolder select info ).ToArray());

	//					retValue = new AlbumSupportInfo(( from DbArtwork artwork in mDatabase.Database
	//													  where artwork.FolderLocation == fileTrack.ParentFolder && artwork.ArtworkType == ArtworkTypes.AlbumCover select artwork ).ToArray(),
	//												    ( from DbArtwork artwork in mDatabase.Database
	//													  where artwork.FolderLocation == fileTrack.ParentFolder && artwork.ArtworkType == ArtworkTypes.AlbumOther select artwork ).ToArray(),
	//													( from DbTextInfo info in mDatabase.Database where info.FolderLocation == fileTrack.ParentFolder select info ).ToArray());
					}
				}
			}

			return( retValue );
		}
	}
}
