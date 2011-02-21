using System.Collections.Generic;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ArtistAlbumConfigViewModel : DialogModelBase {
		public	IEnumerable<ViewSortStrategy>	ArtistSorts { get; private set; }
		public	ViewSortStrategy				SelectedArtistSort { get; set; }
		public	IEnumerable<ViewSortStrategy>	AlbumSorts { get; private set; }
		public	ViewSortStrategy				SelectedAlbumSort { get; set; }

		public ArtistAlbumConfigViewModel( IEnumerable<ViewSortStrategy> artistSorts, ViewSortStrategy currentArtistSort,
										   IEnumerable<ViewSortStrategy> albumSorts, ViewSortStrategy currentAlbumSort ) {
			ArtistSorts = artistSorts;
			SelectedArtistSort = currentArtistSort;
			AlbumSorts = albumSorts;
			SelectedAlbumSort = currentAlbumSort;
		}
	}
}
