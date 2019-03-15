using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.DataProviders {
	public class AlbumsByArtist : AbstractIndexCreationTask<DbAlbum> {
		public AlbumsByArtist() {
			Map = albums => from album in albums select new { album.Artist };
		}
	}

	internal class AlbumProvider : BaseProvider<DbAlbum>, IAlbumProvider {
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public AlbumProvider( IDbFactory databaseFactory,
							  IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider, ITagAssociationProvider tagAssociationProvider, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
			mArtworkProvider = artworkProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mTextInfoProvider = textInfoProvider;
		}

		public void AddAlbum( DbAlbum album ) {
			Database.Add( album );
		}

		public void DeleteAlbum( DbAlbum album ) {
			Database.Delete( album );
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( Database.Get( dbid ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			return( Database.Get( track.Album ));
		}

		public IDataProviderList<DbAlbum> GetAllAlbums() {
			return( Database.FindAll());
		}

		public IDataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			return( GetAlbumList( forArtist.DbId ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			return( Database.Find( album => album.Artist == artistId ));
		}

		public IDataProviderList<DbAlbum> GetFavoriteAlbums() {
			return( Database.Find( album => album.IsFavorite ));
		}

		public IDataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( new RavenDataUpdateShell<DbAlbum>( album => Database.Update( album ), Database.Get( albumId )));
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			return( new AlbumSupportInfo( mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumCover ),
										  mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumArtwork ),
										  mTextInfoProvider.GetAlbumTextInfo( albumId )));
		}

	    public AlbumArtworkInfo GetAlbumArtworkInfo(long albumId) {
	        return (new AlbumArtworkInfo( mArtworkProvider.GetAlbumArtworkInfo( albumId, ContentType.AlbumCover ),
	                                      mArtworkProvider.GetAlbumArtworkInfo( albumId, ContentType.AlbumArtwork )));
	    }

        public IDataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetTagList( eTagGroup.User, categoryId )) {
					retValue = new RavenSimpleDataProviderList<long>( from DbTagAssociation tag in tagList.List.ToList() select tag.AlbumId );
				}
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "GetTagList for category:{0}", categoryId ), ex );
			}

			return ( retValue );
		}

		public IDataProviderList<long> GetAlbumCategories( long albumId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetAlbumTagList( albumId, eTagGroup.User ) ) {
					retValue = new RavenSimpleDataProviderList<long>( from DbTagAssociation assoc in tagList.List.ToList() select assoc.TagId );
				}
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "GetAlbumTagList for album:{0}", albumId ), ex );
			}

			return ( retValue );
		}

		public void SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories ) {
			using( var currentCategories = GetAlbumCategories( albumId )) {
				var categoryList = currentCategories.List.ToList();
				var removeList = categoryList.Where( tagId => !categories.Contains( tagId )).ToList();
				var addList = categories.Where( tagId => !categoryList.Contains( tagId )).ToList();

				if(( removeList.Count > 0 ) ||
				   ( addList.Count > 0 )) {
					try {
						foreach( var tagId in removeList ) {
							var association = mTagAssociationProvider.GetAlbumTagAssociation( albumId, tagId );
							if( association != null ) {
								mTagAssociationProvider.RemoveAssociation( association.DbId );
							}
						}

						foreach( var tagId in addList ) {
							mTagAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, tagId, artistId, albumId ) );
						}
					}
					catch( Exception ex ) {
						Log.LogException( string.Format( "SetAlbumCategories for artist:{0}, album:{1}", artistId, albumId ), ex );
					}
				}
			}
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
