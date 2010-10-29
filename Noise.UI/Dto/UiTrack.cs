using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiTrack : ViewModelBase {
		public long				DbId { get; set; }
		public string			Name { get; set; }
		public string			Performer { get; set; }
		public long				Album { get; set; }
		public TimeSpan			Duration { get; set; }
		public Int32			Bitrate { get; set; }
		public Int32			SampleRate { get; set; }
		public Int16			Channels { get; set; }
		public Int16			Rating { get; set; }
		public UInt16			TrackNumber { get; set; }
		public string			VolumeName { get; set; }
		public UInt32			PublishedYear { get; set; }
		public DateTime			DateAdded { get; set; }
		public eAudioEncoding	Encoding { get; set; }
		public string			CalculatedGenre { get; set; }
		public string			ExternalGenre { get; set; }
		public string			UserGenre { get; set; }
		public bool				IsFavorite { get; set; }

		private readonly Action<long>	mPlayAction;
		private readonly Action<long>	mSelectAction;

		public UiTrack() {
		}

		public UiTrack( Action<long> playAction, Action<long> selectAction ) {
			mPlayAction = playAction;
			mSelectAction = selectAction;
		}

		public bool IsSelected {
			get{ return( Get( () => IsSelected )); }
			set{ 
				Set( () => IsSelected, value ); 

				if(( value ) &&
				   ( mSelectAction != null )) {
					mSelectAction( DbId );
				}
			}
		}

		public void Execute_Play( object sender ) {
			if( mPlayAction != null ) {
				mPlayAction( DbId );
			}
		}

		public bool CanExecute_Play() {
			return( mPlayAction != null );
		}
	}
}
