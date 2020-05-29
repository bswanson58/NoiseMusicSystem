using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;

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
        public ePlayAdjacentStrategy    PlayAdjacentStrategy { get; set; }
        public bool                     DoNotStrategyPlay { get; set; }

        public string                   Genre => DisplayGenre != null ? DisplayGenre.Name : String.Empty;
        public bool                     HasTags => mTags.Any();
        public bool                     HasStrategyOptions => ( PlayAdjacentStrategy != ePlayAdjacentStrategy.None ) || DoNotStrategyPlay;
        public string                   TagsTooltip => mTags.Any() ? string.Join( Environment.NewLine, mTags ) : "Associate File Tags";
        public CombinedPlayStrategy     CombinedPlayStrategy => new CombinedPlayStrategy( PlayAdjacentStrategy, DoNotStrategyPlay );

        public  DelegateCommand         Play { get; }
        public  DelegateCommand         Edit { get; }
        public  DelegateCommand         StrategyOptions { get; }
        public  DelegateCommand         FocusRequest { get; }

        private readonly List<string>   mTags;
		private readonly Action<long>	mPlayAction;
		private readonly Action<long>	mEditAction;
        private readonly Action<long>   mStrategyAction;
        private readonly Action<long>   mFocusRequest;

        protected UiTrack() { }

		public UiTrack( Action<long> playAction, Action<long> editAction, Action<long> strategyEdit, Action<long> focusRequest = null ) {
			mPlayAction = playAction;
			mEditAction = editAction;
            mStrategyAction = strategyEdit;
            mFocusRequest = focusRequest;

            mTags = new List<string>();

            Play = new DelegateCommand( OnPlay, CanPlay );
            Edit = new DelegateCommand( OnEdit, CanEdit );
            StrategyOptions = new DelegateCommand( OnStrategyOptions, CanEditStrategyOptions );
            FocusRequest = new DelegateCommand( OnFocusRequest, CanFocusRequest );
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

        private void OnPlay() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

            mPlayAction?.Invoke( DbId );
        }

		private bool CanPlay() {
			return( mPlayAction != null );
		}

		private void OnEdit() {
            mEditAction?.Invoke( DbId );
        }

		private bool CanEdit() {
			return( mEditAction != null );
		}

        private void OnStrategyOptions() {
            mStrategyAction?.Invoke( DbId );
        }

        private bool CanEditStrategyOptions() {
            return mStrategyAction != null;
        }

        private void OnFocusRequest() {
            mFocusRequest?.Invoke( DbId );
        }

        private bool CanFocusRequest() {
            return mFocusRequest != null;
        }

        public void SetTags( IEnumerable<string> tags ) {
            mTags.Clear();
            mTags.AddRange( tags );

            RaisePropertyChanged( () => HasTags );
            RaisePropertyChanged( () => TagsTooltip );
        }

        public void SetStrategyOption( ePlayAdjacentStrategy strategy, bool doNotPlay ) {
            PlayAdjacentStrategy = strategy;
            DoNotStrategyPlay = doNotPlay;

            RaisePropertyChanged( () => CombinedPlayStrategy );
            RaisePropertyChanged( () => PlayAdjacentStrategy );
            RaisePropertyChanged( () => HasStrategyOptions );
        }

        public void SetPlayingStatus( PlayingItem item ) {
            IsPlaying = DbId.Equals( item.Track );
        }
    }
}
