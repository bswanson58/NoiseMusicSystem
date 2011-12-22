using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class AlbumProvider : BaseDataProvider<DbAlbum>, IAlbumProvider {
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public AlbumProvider( IDatabaseManager databaseManager, IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
							  ITagAssociationProvider tagAssociationProvider ) :
			base( databaseManager ) {
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagAssociationProvider = tagAssociationProvider;
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( TryGetItem(  "SELECT DbAlbum Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", dbid }}, "Exception - GetAlbum:" ));
		}

		public DataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			return( GetAlbumList( forArtist.DbId ));
		}

		public DataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			return( TryGetList( "SELECT DbAlbum WHERE Artist = @artistId", new Dictionary<string, object>{{ "artistId", artistId }}, "Exception - GetAlbumList" ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			return( GetAlbum( track.Album ));
		}

		public DataProviderList<DbAlbum> GetFavoriteAlbums() {
			return( TryGetList( "SELECT DbAlbum WHERE IsFavorite = true", "Exception - GetFavoriteAlbums" ));
		}

		public DataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( GetUpdateShell( "SELECT DbAlbum Where DbId = @albumId", new Dictionary<string, object> {{ "albumId", albumId }} ));
		}

		public DataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			DataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetTagList( eTagGroup.User, categoryId )) {
					retValue = new DataProviderList<long>( null, ( from DbTagAssociation tag in tagList.List select tag.AlbumId ).ToList());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetTagList", ex );
			}

			return( retValue );
		}

		public DataProviderList<long> GetAlbumCategories( long albumId ) {
			DataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetAlbumTagList( albumId, eTagGroup.User )) {
					retValue = new DataProviderList<long>( null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetAlbumCategories", ex );
			}

			return( retValue );
		}

		public void SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories ) {
			var currentCategories = GetAlbumCategories( albumId );
			var removeList = currentCategories.List.Where( tagId => !categories.Contains( tagId )).ToList();
			var addList = categories.Where( tagId => !currentCategories.List.Contains( tagId )).ToList();

			if(( removeList.Count > 0 ) ||
			   ( addList.Count > 0 )) {
				try {
					foreach( var tagId in removeList ) {
						mTagAssociationProvider.RemoveAssociation( tagId );
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

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			return( new AlbumSupportInfo( mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumCover ),
										  mArtworkProvider.GetAlbumArtwork( albumId, ContentType.AlbumArtwork ),
										  mTextInfoProvider.GetAlbumTextInfo( albumId )));
		}
	}
}
