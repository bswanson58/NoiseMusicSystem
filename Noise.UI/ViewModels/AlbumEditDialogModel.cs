using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class AlbumEditDialogModel : DialogModelBase {
		public	DbAlbum		Album { get; private set; }
		public	bool		UpdateFileTags { get; set; }

		public AlbumEditDialogModel( DbAlbum album ) {
			Album = album;
		}
	}
}
