using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class AlbumEditDialogModel : DialogModelBase {
		public	UiAlbum	Album { get; private set; }
		public	bool	UpdateFileTags { get; set; }
		
		public AlbumEditDialogModel( UiAlbum album ) {
			Album = album;
		}
	}
}
