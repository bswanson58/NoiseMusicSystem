using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class AlbumProvider : BaseDataProvider<DbAlbum>, IAlbumProvider {
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public AlbumProvider( IDatabaseManager databaseManager,
							  IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider, ITagAssociationProvider tagAssociationProvider ) :
			base( databaseManager ) {
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagAssociationProvider = tagAssociationProvider;
		}

		public void AddAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			InsertItem( album );
		}

		public void DeleteAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			DeleteItem( album );
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( TryGetItem(  "SELECT DbAlbum Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", dbid }}, "Exception - GetAlbum:" ));
		}

		public IDataProviderList<DbAlbum> GetAllAlbums() {
			return( TryGetList( "SELECT DbAlbum", "GetAllAlbums" ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			return( GetAlbumList( forArtist.DbId ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			return( TryGetList( "SELECT DbAlbum WHERE Artist = @artistId", new Dictionary<string, object>{{ "artistId", artistId }}, "Exception - GetAlbumList" ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			return( GetAlbum( track.Album ));
		}

		public IDataProviderList<DbAlbum> GetFavoriteAlbums() {
			return( TryGetList( "SELECT DbAlbum WHERE IsFavorite = true", "Exception - GetFavoriteAlbums" ));
		}

		public IDataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( GetUpdateShell( "SELECT DbAlbum Where DbId = @albumId", new Dictionary<string, object> {{ "albumId", albumId }} ));
		}

		public IDataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetTagList( eTagGroup.User, categoryId )) {
					retValue = new EloqueraProviderList<long>( null, ( from DbTagAssociation tag in tagList.List select tag.AlbumId ).ToList());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetTagList", ex );
			}

			return( retValue );
		}

		public IDataProviderList<long> GetAlbumCategories( long albumId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetAlbumTagList( albumId, eTagGroup.User )) {
					retValue = new EloqueraProviderList<long>( null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumCategories", ex );
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
						NoiseLogger.Current.LogException( "Exception - SetAlbumCategories", ex );
					}
				}
			}
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			return( new AlbumSupportInfo( mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumCover ),
										  mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumArtwork ),
										  mTextInfoProvider.GetAlbumTextInfo( albumId )));
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT DbAlbum" ));
		}
	}
}
