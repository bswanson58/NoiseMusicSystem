using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class ArtistProvider : BaseProvider<DbArtist>, IArtistProvider {
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private readonly ITagAssociationProvider		mTagAssociationProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;

		public ArtistProvider( IContextProvider contextProvider, IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
							   ITagAssociationProvider tagAssociationProvider,	IAssociatedItemListProvider associatedItemListProvider ) :
			base( contextProvider ) {
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mAssociationProvider = associatedItemListProvider;
		}

		public void AddArtist( DbArtist artist ) {
			AddItem( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			RemoveItem( artist );
		}

		public DbArtist GetArtist( long dbid ) {
			return( GetItemByKey( dbid ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			return( GetItemByKey( album.Artist ));
		}

		public IDataProviderList<DbArtist> GetArtistList() {
			return( GetListShell());
		}

		public IDataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			IEnumerable<DbArtist>	artistList;

			using( var context = CreateContext()) {
				artistList = Set( context ).ToList();
			}

			return( new EfProviderList<DbArtist>( null, artistList.Where( filter.ArtistMatch )));
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			var context = CreateContext();

			return( new EfProviderList<DbArtist>( context, Set( context ).Where( entity => entity.LastChangeTicks > changedSince )));
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			var context = CreateContext();

			return( new EfProviderList<DbArtist>( context, Set( context ).Where( entity => entity.IsFavorite )));
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			return( GetUpdateShell( artistId ));
		}

		public void UpdateArtistLastChanged( long artistId ) {
			using( var context = CreateContext()) {
				var artist = GetItemByKey( context, artistId );

				if( artist != null ) {
					artist.UpdateLastChange();

					context.SaveChanges();
				}
			}
		}

		public IDataProviderList<long> GetArtistCategories( long artistId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetArtistTagList( artistId, eTagGroup.User )) {
					retValue = new EfProviderList<long>( null, ( from DbTagAssociation assoc in tagList.List select assoc.TagId ).ToList());
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
			return( GetEntityCount());
		}
	}
}
