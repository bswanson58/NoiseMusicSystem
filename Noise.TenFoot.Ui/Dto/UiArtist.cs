using Noise.Infrastructure.Dto;

namespace Noise.TenFoot.Ui.Dto {
	public class UiArtist : UiBase {
		public	string		Name { get; set; }
		public	string		DisplayName { get; set; }
		public	string		SortName { get; set; }
		public	int			AlbumCount { get; set; }
		public	Artwork		ArtistImage { get; set; }
	}
}
