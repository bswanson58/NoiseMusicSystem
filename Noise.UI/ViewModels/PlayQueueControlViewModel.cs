using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class PlayQueueControlViewModel : AutomaticCommandBase,
                                             IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator			                mEventAggregator;
        private readonly IArtistProvider                            mArtistProvider;
        private readonly IGenreProvider                             mGenreProvider;
        private readonly ITagProvider                               mTagProvider;
        private readonly IPlayListProvider                          mPlayListProvider;
		private readonly IInternetStreamProvider	                mStreamProvider;
		private readonly IPlayQueue					                mPlayQueue;
        private readonly IDialogService                             mDialogService;
		private	readonly BindableCollection<ExhaustedStrategyItem>	mExhaustedStrategies;
		private readonly BindableCollection<PlayStrategyItem>		mPlayStrategies;
		private TimeSpan							                mTotalTime;
		private TimeSpan							                mRemainingTime;

        public PlayQueueControlViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IDialogService dialogService,
                                          IArtistProvider artistProvider, IGenreProvider genreProvider, ITagProvider tagProvider,
                                          IInternetStreamProvider streamProvider, IPlayListProvider playListProvider ) {
            mArtistProvider = artistProvider;
            mGenreProvider = genreProvider;
            mStreamProvider = streamProvider;
            mPlayListProvider = playListProvider;
            mTagProvider = tagProvider;
            mPlayQueue = playQueue;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>{
			                                    new PlayStrategyItem( ePlayStrategy.Next, "Normal" ),
												new PlayStrategyItem( ePlayStrategy.Random, "Random" ),
												new PlayStrategyItem( ePlayStrategy.TwoFers, "2 Fers" ),
												new PlayStrategyItem( ePlayStrategy.FeaturedArtists, "Featured Artist" ),
												new PlayStrategyItem( ePlayStrategy.NewReleases, "New Releases" )};

			mExhaustedStrategies = new BindableCollection<ExhaustedStrategyItem>{
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Stop, "Stop" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Replay, "Replay" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayArtist, "Play Artist..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayArtistGenre, "Play Genre..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayCategory, "Play Category..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayFavorites, "Play Favorites" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlaySimilar, "Play Similar" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.SeldomPlayedArtists, "Seldom Played" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayList, "Playlist..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayStream, "Radio Station..." )};

			mEventAggregator.Subscribe( this );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mPlayQueue.SetPlayExhaustedStrategy( configuration.PlayExhaustedStrategy,
													 PlayStrategyParametersFactory.FromString( configuration.PlayExhaustedParameters ));
				mPlayQueue.SetPlayStrategy( configuration.PlayStrategy,
													 PlayStrategyParametersFactory.FromString( configuration.PlayStrategyParameters ));
			}
        }

        public void Handle( Events.PlayQueueChanged eventArgs ) {
			UpdatePlayTimes();

			PlayQueueChangedFlag++;
		}

        public void Handle( Events.PlaybackTrackStarted args ) {
            UpdatePlayTimes();

            RaiseCanExecuteChangedEvent( "CanExecute_ClearPlayed" );
        }

        private void UpdatePlayTimes() {
			mTotalTime = new TimeSpan();
			mRemainingTime = new TimeSpan();

			foreach( var track in mPlayQueue.PlayList ) {
				var	trackTime = track.Track != null ? track.Track.Duration : new TimeSpan();

				mTotalTime = mTotalTime.Add( trackTime );

				if((!track.HasPlayed ) ||
				   ( track.IsPlaying )) {
					mRemainingTime = mRemainingTime.Add( trackTime );
				}
			}

			RaisePropertyChanged( () => TotalTime );
			RaisePropertyChanged( () => RemainingTime );
        }

		public int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		public void Execute_StartStrategy() {
			mPlayQueue.StartPlayStrategy();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_StartStrategy() {
			return( mPlayQueue.CanStartPlayStrategy );
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan TotalTime {
			get{ return( mTotalTime ); }
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan RemainingTime {
			get{ return( mRemainingTime ); }
		}

		public void Execute_ClearQueue( object sender ) {
			mPlayQueue.ClearQueue();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			return(!mPlayQueue.IsQueueEmpty );
		}

		public void Execute_ClearPlayed() {
			mPlayQueue.RemovePlayedTracks();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearPlayed() {
			return( mPlayQueue.PlayedTrackCount > 0 );
		}

		public BindableCollection<ExhaustedStrategyItem> ExhaustedStrategyList {
			get{ return( mExhaustedStrategies ); }
		}

		public ePlayExhaustedStrategy ExhaustedStrategy {
			get{ return( mPlayQueue.PlayExhaustedStrategy ); }
			set {
				IPlayStrategyParameters	parameters;

				if( PromptForExhaustedStrategyItem( value, out parameters )) {
					mPlayQueue.SetPlayExhaustedStrategy( value, parameters );
					RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );

					var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					if( configuration != null ) {
						configuration.PlayExhaustedStrategy = value;
						configuration.PlayExhaustedParameters = PlayStrategyParametersFactory.ToString( parameters );

						NoiseSystemConfiguration.Current.Save( configuration );
					}
				}
			}
		}

		private bool PromptForExhaustedStrategyItem( ePlayExhaustedStrategy strategy, out IPlayStrategyParameters parameters ) {
			var retValue = false;
			parameters = null;

			if(( strategy == ePlayExhaustedStrategy.PlayStream ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayArtist ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayCategory ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayList ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayArtistGenre ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayGenre )) {
				if( strategy == ePlayExhaustedStrategy.PlayList ) {
					var dialogModel = new SelectPlayListDialogModel( mPlayListProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectPlayList, dialogModel ) == true ) {
						if( dialogModel.Parameters != null ) {
							parameters = dialogModel.Parameters;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayStream ) {
					var	dialogModel = new SelectStreamDialogModel( mStreamProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectStream, dialogModel ) == true ) {
						if( dialogModel.Parameters != null ) {
							parameters = dialogModel.Parameters;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayArtist ) {
					var dialogModel = new SelectArtistDialogModel( mArtistProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectArtist, dialogModel ) == true ) {
						if( dialogModel.Parameters != null ) {
							parameters = dialogModel.Parameters;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayCategory ) {
					var dialogModel = new SelectCategoryDialogModel( mTagProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectCategory, dialogModel ) == true ) {
						if( dialogModel.Parameters != null ) {
							parameters = dialogModel.Parameters;
							retValue = true;
						}
					}
				}

				if(( strategy == ePlayExhaustedStrategy.PlayGenre ) ||
				   ( strategy == ePlayExhaustedStrategy.PlayArtistGenre )) {
					var dialogModel = new SelectGenreDialogModel( mGenreProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectGenre, dialogModel ) == true ) {
						if( dialogModel.Parameters != null ) {
							parameters = dialogModel.Parameters;
							retValue = true;
						}
					}
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}

		public BindableCollection<PlayStrategyItem> PlayStrategyList {
			get{ return( mPlayStrategies ); }
		}

		public ePlayStrategy PlayStrategy {
			get{ return( mPlayQueue.PlayStrategy ); }
			set {
				IPlayStrategyParameters	parameters;

				if( PromptForPlayStrategyItem( value, out parameters )) {
					mPlayQueue.SetPlayStrategy( value, parameters );

					var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					if( configuration != null ) {
						configuration.PlayStrategy = value;
						configuration.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( parameters );

						NoiseSystemConfiguration.Current.Save( configuration );
					}
				}
			}
		}

		private bool PromptForPlayStrategyItem( ePlayStrategy strategy, out IPlayStrategyParameters parameters ) {
			var retValue = false;
			parameters = null;

			if( strategy == ePlayStrategy.FeaturedArtists ) {
				var dialogModel = new FeaturedArtistsDialogModel( mArtistProvider );

				if( mDialogService.ShowDialog( DialogNames.FeaturedArtistsSelect, dialogModel ) == true ) {
					if( dialogModel.Parameters != null ) {
						parameters = dialogModel.Parameters;
						retValue = true;
					}
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}
    }
}
