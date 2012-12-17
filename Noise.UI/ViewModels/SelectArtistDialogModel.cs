using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.ViewModels {
	class SelectArtistDialogModel {
		private readonly List<DbArtist>	mArtistList;

		public	DbArtist				SelectedItem { get; set; }

		public SelectArtistDialogModel( IArtistProvider artistProvider ) {
			using( var artistList = artistProvider.GetArtistList()) {
				mArtistList = new List<DbArtist>( from artist in artistList.List orderby artist.Name select  artist );
			}
		}

		public IEnumerable<DbArtist> ArtistList {
			get{ return( mArtistList ); }
		}
	}
}
