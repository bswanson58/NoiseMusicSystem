
namespace Noise.Core.DataExchange.Dto {
	public class ExportStream : ExportBase {
		public string			Description { get; set; }
		public string			Url { get; set; }
		public bool				IsPlaylistWrapped { get; set; }
		public string			Website { get; set; }

		public ExportStream() {
		}

		public ExportStream( long originDb, string streamName, string description, string url, bool isPlayList, string website ) :
			base( streamName, originDb ) {
			Description = description;
			Url = url;
			IsPlaylistWrapped = isPlayList;
			Website = website;
		}
	}
}
