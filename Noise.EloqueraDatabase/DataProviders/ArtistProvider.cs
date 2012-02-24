using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class ArtistProvider : BaseDataProvider<DbArtist>, IArtistProvider {
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private readonly ITagAssociationProvider		mTagAssociationProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;

		public ArtistProvider( IEloqueraManager databaseManager,
							   IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
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

		public IDataProviderList<DbArtist> GetArtistList() {
			return( TryGetList( "SELECT DbArtist", "Exception - GetArtistList:" ));
		}

		public IDataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			IDataProviderList<DbArtist>	retValue = null;

			try {
				using( var artistList = GetList( "SELECT DbArtist" )) {
					retValue = new EloqueraProviderList<DbArtist>( null, ( from artist in artistList.List where filter.ArtistMatch( artist ) select artist ).ToList());
				}			
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}

			return( retValue );
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			return( TryGetList( "SELECT DbArtist WHERE LastChangeTicks > @changedSince", new Dictionary<string, object> {{ "changedSince", changedSince }}, "GetChangedArtists" ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			return( TryGetItem( "SELECT DbArtist Where DbId = @artistId", new Dictionary<string, object> {{ "artistId", album.Artist }}, "Exception - GetArtistForAlbum:" ));
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			return( TryGetList( "SELECT DbArtist WHERE IsFavorite = true",  "Exception - GetFavoriteArtists:" ));
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			var retValue = GetUpdateShell( "SELECT DbArtist Where DbId = @artistId", new Dictionary<string, object> {{ "artistId", artistId }} );

			retValue.Item.UpdateLastChange();

			return( retValue );
		}

		public void UpdateArtistLastChanged( long artistId ) {
			using( var artistUpdate = GetArtistForUpdate( artistId )) {
				artistUpdate.Update();
			}
		}

		public IDataProviderList<long> GetArtistCategories( long artistId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetArtistTagList( artistId, eTagGroup.User )) {
					retValue = new EloqueraProviderList<long>( null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
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
