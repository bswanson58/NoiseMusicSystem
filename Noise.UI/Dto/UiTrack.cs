using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Track = {Name}")]
	public class UiTrack : UiBase {
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
		public DbGenre			DisplayGenre { get; set; }

		private readonly Action<long>	mPlayAction;
		private readonly Action<long>	mEditAction;

		public UiTrack( Action<long> playAction, Action<long> editAction ) {
			mPlayAction = playAction;
			mEditAction = editAction;
		}

		public string Genre {
			get{ return( DisplayGenre != null ? DisplayGenre.Name : "" ); }
		}

		public bool IsSelected {
			get{ return( Get( () => IsSelected )); }
			set{ Set( () => IsSelected, value ); }
		}

		public void Execute_Play() {
			if( mPlayAction != null ) {
				mPlayAction( DbId );
			}
		}

		public bool CanExecute_Play() {
			return( mPlayAction != null );
		}

		public void Execute_Edit() {
			if( mEditAction != null ) {
				mEditAction( DbId );
			}
		}

		public bool CanExecute_Edit() {
			return( mEditAction != null );
		}
	}
}
