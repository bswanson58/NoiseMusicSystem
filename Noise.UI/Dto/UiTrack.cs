using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
	[DebuggerDisplay("Track = {" + nameof( Name ) + "}")]
	public class UiTrack : UiBase, IPlayingItem {
		public string			        Name { get; set; }
		public string			        Performer { get; set; }
		public long				        Album { get; set; }
		public TimeSpan			        Duration { get; set; }
		public Int32			        Bitrate { get; set; }
		public Int32			        SampleRate { get; set; }
		public Int16			        Channels { get; set; }
		public Int16			        Rating { get; set; }
		public Int16			        TrackNumber { get; set; }
		public string			        VolumeName { get; set; }
		public Int32			        PublishedYear { get; set; }
		public DateTime			        DateAdded { get; set; }
		public eAudioEncoding	        Encoding { get; set; }
		public string			        CalculatedGenre { get; set; }
		public string			        ExternalGenre { get; set; }
		public string			        UserGenre { get; set; }
		public bool				        IsFavorite { get; set; }
		public DbGenre			        DisplayGenre { get; set; }
        public ePlayAdjacentStrategy    PlayStrategyOptions { get; set; }
        public bool                     DoNotStrategyPlay { get; set; }

        public string                   Genre => DisplayGenre != null ? DisplayGenre.Name : String.Empty;
        public bool                     HasTags => mTags.Any();
        public bool                     HasStrategyOptions => ( PlayStrategyOptions != ePlayAdjacentStrategy.None ) || DoNotStrategyPlay;
        public string                   TagsTooltip => mTags.Any() ? string.Join( Environment.NewLine, mTags ) : "Associate File Tags";
        public CombinedPlayStrategy     CombinedPlayStrategy => new CombinedPlayStrategy( PlayStrategyOptions, DoNotStrategyPlay );

        private readonly List<string>   mTags;
		private readonly Action<long>	mPlayAction;
		private readonly Action<long>	mEditAction;
        private readonly Action<long>   mStrategyAction;

        protected UiTrack() { }

		public UiTrack( Action<long> playAction, Action<long> editAction, Action<long> strategyEdit ) {
			mPlayAction = playAction;
			mEditAction = editAction;
            mStrategyAction = strategyEdit;

            mTags = new List<string>();
		}

        public bool IsHighlighted {
            get {  return( Get(() => IsHighlighted ));}
            set {  Set(() => IsHighlighted, value );}
        }

        public bool IsSelected {
			get{ return( Get( () => IsSelected )); }
			set{ Set( () => IsSelected, value ); }
		}

        public bool IsPlaying {
            get{ return( Get( () => IsPlaying )); }
            set{ Set( () => IsPlaying, value ); }
        }

        public void Execute_Play() {
            mPlayAction?.Invoke( DbId );
        }

		public bool CanExecute_Play() {
			return( mPlayAction != null );
		}

		public void Execute_Edit() {
            mEditAction?.Invoke( DbId );
        }

		public bool CanExecute_Edit() {
			return( mEditAction != null );
		}

        public void Execute_StrategyOptions() {
            mStrategyAction?.Invoke( DbId );
        }

        public bool CanExecute_StrategyOptions() {
            return mStrategyAction != null;
        }

        public void SetTags( IEnumerable<string> tags ) {
            mTags.Clear();
            mTags.AddRange( tags );

            RaisePropertyChanged( () => HasTags );
            RaisePropertyChanged( () => TagsTooltip );
        }

        public void SetStrategyOption( ePlayAdjacentStrategy strategy, bool doNotPlay ) {
            PlayStrategyOptions = strategy;
            DoNotStrategyPlay = doNotPlay;

            RaisePropertyChanged( () => CombinedPlayStrategy );
            RaisePropertyChanged( () => PlayStrategyOptions );
            RaisePropertyChanged( () => HasStrategyOptions );
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = DbId.Equals( item.Track );
        }
    }
}
