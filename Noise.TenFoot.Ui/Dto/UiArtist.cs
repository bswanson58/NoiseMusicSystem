using Noise.Infrastructure.Dto;

namespace Noise.TenFoot.Ui.Dto {
	public class UiArtist : UiBase {
		private Artwork		mArtistImage;

		public	string		Name { get; set; }
		public	string		DisplayName { get; set; }
		public	string		SortName { get; set; }
		public	int			AlbumCount { get; set; }

		public Artwork ArtistImage {
			get{ return( mArtistImage ); }
			set{
				mArtistImage = value;
				RaisePropertyChanged( () => ArtistImage );
			}
		}
	}
}
