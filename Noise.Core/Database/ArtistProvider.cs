using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class ArtistProvider : BaseDataProvider<DbArtist>, IArtistProvider {
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private readonly ITagAssociationProvider		mTagAssociationProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;

		public ArtistProvider( IDatabaseManager databaseManager, IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
							   ITagAssociationProvider tagAssociationProvider,	IAssociatedItemListProvider associatedItemListProvider )
			: base( databaseManager ) {
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mAssociationProvider = associatedItemListProvider;
		}

		// IArtistProvider members
		public void AddArtist( DbArtist artist ) {
			Condition.Requires( artist ).IsNotNull();

			InsertItem( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			Condition.Requires( artist ).IsNotNull();

			DeleteItem( artist );
		}

		public DbArtist GetArtist( long dbid ) {
			return( TryGetItem(  "SELECT DbArtist Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", dbid }}, "Exception - GetArtist:" ));
		}

		public DataProviderList<DbArtist> GetArtistList() {
			return( TryGetList( "SELECT DbArtist", "Exception - GetArtistList:" ));
		}

		public DataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			DataProviderList<DbArtist>	retValue = null;

			try {
				using( var artistList = GetList( "SELECT DbArtist" )) {
					retValue = new DataProviderList<DbArtist>( "", null, ( from artist in artistList.List where filter.ArtistMatch( artist ) select artist ).ToList());
				}			
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}

			return( retValue );
		}

		public DataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			return( TryGetList( "SELECT DbArtist WHERE LastChangeTicks > @changedSince", new Dictionary<string, object> {{ "changedSince", changedSince }}, "GetChangedArtists" ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			return( TryGetItem( "SELECT DbArtist Where DbId = @artistId", new Dictionary<string, object> {{ "artistId", album.Artist }}, "Exception - GetArtistForAlbum:" ));
		}

		public DataProviderList<DbArtist> GetFavoriteArtists() {
			return( TryGetList( "SELECT DbArtist WHERE IsFavorite = true",  "Exception - GetFavoriteArtists:" ));
		}

		public DataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			var retValue = GetUpdateShell( "SELECT DbArtist Where DbId = @artistId", new Dictionary<string, object> {{ "artistId", artistId }} );

			retValue.Item.UpdateLastChange();

			return( retValue );
		}

		public void UpdateArtistLastChanged( long artistId ) {
			using( var artistUpdate = GetArtistForUpdate( artistId )) {
				artistUpdate.Update();
			}
		}

		public DataProviderList<long> GetArtistCategories( long artistId ) {
			DataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetArtistTagList( artistId, eTagGroup.User )) {
					retValue = new DataProviderList<long>( "", null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetArtistCategories", ex );
			}

			return( retValue );
		}

		public ArtistSupportInfo GetArtistSupportInfo( long artistId ) {
			return( new ArtistSupportInfo( mTextInfoProvider.GetArtistTextInfo( artistId, ContentType.Biography ),
										   mArtworkProvider.GetArtistArtwork( artistId, ContentType.ArtistPrimaryImage ),
										   mAssociationProvider.GetAssociatedItems( artistId, ContentType.SimilarArtists ),
										   mAssociationProvider.GetAssociatedItems( artistId, ContentType.TopAlbums ),
										   mAssociationProvider.GetAssociatedItems( artistId, ContentType.BandMembers ) ));
		}

		public long GetItemCount() {
			return( GetItemCount( "SELECT DbArtist" ));
		}
	}
}
