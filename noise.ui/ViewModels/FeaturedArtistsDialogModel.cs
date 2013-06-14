using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	public class FeaturedArtistsDialogModel {
		private readonly List<DbArtist>		mArtistList;
		private	DbArtist					mSelectedItem;

		public	IPlayStrategyParameters	Parameters { get; private set; }

		public FeaturedArtistsDialogModel( IArtistProvider artistProvider ) {
			using( var artistList = artistProvider.GetArtistList()) {
				mArtistList = new List<DbArtist>( from artist in artistList.List orderby artist.Name select  artist );
			}
		}

		public IEnumerable<DbArtist> ArtistList {
			get{ return( mArtistList ); }
		}

		public DbArtist SelectedItem {
			get{ return( mSelectedItem ); }
			set {
				mSelectedItem = value;

				if( mSelectedItem != null ) {
					Parameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayArtist ) { DbItemId = mSelectedItem.DbId };
				}
			}
		}
	}
}
