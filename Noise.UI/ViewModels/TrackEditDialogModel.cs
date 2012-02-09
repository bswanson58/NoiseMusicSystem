using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class TrackEditDialogModel : DialogModelBase {
		public	DbTrack	Track { get; private set; }
		public	bool	UpdateFileTags { get; set; }

		public TrackEditDialogModel( DbTrack track ) {
			Track = track;
		}
	}
}
