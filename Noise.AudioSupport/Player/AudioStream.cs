using System;
using Noise.Infrastructure.Dto;

namespace Noise.AudioSupport.Player {
	internal class AudioStream {
		public	int			Channel { get; private set; }
		public	StorageFile PhysicalFile { get; private set; }
		public	string		Url { get; private set; }
		public	bool		IsActive { get; set; }
		public	bool		InSlide { get; set; }
		public	bool		PauseOnSlide { get; set; }
		public	bool		StopOnSlide { get; set; }
		public	bool		Faded { get; set; }
		public	int			MetaDataSync { get; set; }
		public	int			ReplayGainFx { get; set; }
		public	int			SyncEnd { get; set; }
		public	int			SyncNext { get; set; }
		public	int			SyncQueued { get; set; }
		public	int			SyncStalled { get; set; }

		private AudioStream( int channel ) {
			Channel = channel;
		}

		public AudioStream( StorageFile file, int channel ) :
			this( channel ) {
			PhysicalFile = file;
		}

		public AudioStream( string url, int channel ) :
			this( channel ) {
			Url = url;
		}

		public bool IsStream {
			get { return ( !String.IsNullOrWhiteSpace( Url )); }
		}
	}
}
