using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class AlbumProvider : BaseProvider<DbAlbum>, IAlbumProvider {
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public AlbumProvider( IContextProvider contextProvider,
							  IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider, ITagAssociationProvider tagAssociationProvider, ILogDatabase log ) :
			base( contextProvider, log ) {
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagAssociationProvider = tagAssociationProvider;
		}

		public void AddAlbum( DbAlbum album ) {
			AddItem( album );
		}

		public void DeleteAlbum( DbAlbum album ) {
			RemoveItem( album );
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( GetItemByKey( dbid ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			return( GetItemByKey( track.Album ));
		}

		public IDataProviderList<DbAlbum> GetAllAlbums() {
			return( GetListShell());
		}

		public IDataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			return( GetAlbumList( forArtist.DbId ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			var context = CreateContext();

			return( new EfProviderList<DbAlbum>( context, Set( context ).Where( entity => entity.Artist == artistId )));
		}

		public IDataProviderList<DbAlbum> GetFavoriteAlbums() {
			var context = CreateContext();

			return( new EfProviderList<DbAlbum>( context, Set( context ).Where( entity => entity.IsFavorite )));
		}

		public IDataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( GetUpdateShell( albumId ));
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			return( new AlbumSupportInfo( mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumCover ),
										  mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumArtwork ),
										  mTextInfoProvider.GetAlbumTextInfo( albumId )));
		}

        public AlbumArtworkInfo GetAlbumArtworkInfo( long albumId ) {
            return( new AlbumArtworkInfo( mArtworkProvider.GetAlbumArtworkInfo( albumId, ContentType.AlbumCover ),
                                          mArtworkProvider.GetAlbumArtworkInfo( albumId, ContentType.AlbumArtwork )));
        }

		public IDataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetTagList( eTagGroup.User, categoryId )) {
					retValue = new EfProviderList<long>( null, ( from DbTagAssociation tag in tagList.List select tag.AlbumId ).ToList());
				}
			}
			catch( Exception ex ) {
				Log.LogException( "GetTagList", ex );
			}

			return( retValue );
		}

		public IDataProviderList<long> GetAlbumCategories( long albumId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetAlbumTagList( albumId, eTagGroup.User )) {
					retValue = new EfProviderList<long>( null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
				}
			}
			catch( Exception ex ) {
				Log.LogException( "GetAlbumTagList", ex );
			}

			return( retValue );
		}

		public void SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories ) {
			using( var currentCategories = GetAlbumCategories( albumId )) {
				var removeList = currentCategories.List.Where( tagId => !categories.Contains( tagId )).ToList();
				var addList = categories.Where( tagId => !currentCategories.List.Contains( tagId )).ToList();

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
							mTagAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, tagId, artistId, albumId ));
						}
					}
					catch( Exception ex ) {
						Log.LogException( "Adding Associations", ex );
					}
				}
			}
		}

		public long GetItemCount() {
			return( GetEntityCount());
		}
	}
}
