using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyCategory : PlayExhaustedStrategyRandomBase {
		private readonly IAlbumProvider	mAlbumProvider;
		private readonly ITrackProvider	mTrackProvider;
		private readonly ITagProvider	mTagProvider;
		private	readonly List<long>		mAlbums;
		private long					mCategoryId;
		private string					mCategoryName;

		public PlayExhaustedStrategyCategory( IAlbumProvider albumProvider, ITrackProvider trackProvider, ITagProvider tagProvider ) :
			base( ePlayExhaustedStrategy.PlayCategory, "Play Category...", true, "Category", albumProvider, trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTagProvider = tagProvider;

			mAlbums = new List<long>();
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( parameters is PlayStrategyParameterDbId ) {
				var dbParam = parameters as PlayStrategyParameterDbId;

				if( mCategoryId != dbParam.DbItemId ) {
					mAlbums.Clear();
					mCategoryId = dbParam.DbItemId;

					using( var albumList = mAlbumProvider.GetAlbumsInCategory( mCategoryId )) {
						mAlbums.AddRange( from album in albumList.List select album );
					}

					var tag = ( from t in mTagProvider.GetTagList( eTagGroup.User ).List where t.DbId == mCategoryId select t ).FirstOrDefault();
					if( tag != null ) {
						mCategoryName = tag.Name;
					}
				}
			}
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from category {0}", mCategoryName ));
		}

		protected override DbTrack SelectATrack() {
			var albumId = mAlbums.Skip( NextRandom( mAlbums.Count )).FirstOrDefault();
			var album = mAlbumProvider.GetAlbum( albumId );

			return( RandomTrackFromAlbum( album ));
		}
	}
}
