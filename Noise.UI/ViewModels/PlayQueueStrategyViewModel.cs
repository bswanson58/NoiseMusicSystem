using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class PlayQueueStrategyViewModel : AutomaticCommandBase,
                                              IHandle<Events.PlayQueueChanged> {
        private readonly IEventAggregator                           mEventAggregator;
        private readonly IArtistProvider                            mArtistProvider;
        private readonly IGenreProvider                             mGenreProvider;
        private readonly ITagProvider                               mTagProvider;
        private readonly IPlayListProvider                          mPlayListProvider;
		private readonly IInternetStreamProvider	                mStreamProvider;
        private readonly IPlayQueue                                 mPlayQueue;
        private readonly IPlayStrategyFactory                       mPlayStrategyFactory;
        private readonly IDialogService                             mDialogService;

		private	readonly BindableCollection<ExhaustedStrategyItem>	mExhaustedStrategies;
		private readonly BindableCollection<PlayStrategyItem>		mPlayStrategies;

        public PlayQueueStrategyViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IPlayStrategyFactory playStrategyFactory,
            IDialogService dialogService,
                                           IArtistProvider artistProvider, IGenreProvider genreProvider, ITagProvider tagProvider,
                                           IInternetStreamProvider streamProvider, IPlayListProvider playListProvider ) {
            mEventAggregator = eventAggregator;
            mArtistProvider = artistProvider;
            mGenreProvider = genreProvider;
            mStreamProvider = streamProvider;
            mPlayListProvider = playListProvider;
            mTagProvider = tagProvider;
            mPlayQueue = playQueue;
            mPlayStrategyFactory = playStrategyFactory;
            mDialogService = dialogService;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>( from strategy in mPlayStrategyFactory.AvailableStrategies 
                                                                        select new PlayStrategyItem( strategy.StrategyId, strategy.DisplayName ));

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

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mPlayQueue.SetPlayExhaustedStrategy( configuration.PlayExhaustedStrategy,
													 PlayStrategyParametersFactory.FromString( configuration.PlayExhaustedParameters ));
				mPlayQueue.SetPlayStrategy( configuration.PlayStrategy,
													 PlayStrategyParametersFactory.FromString( configuration.PlayStrategyParameters ));
			}

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.PlayQueueChanged eventArgs ) {
            RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );
		}
		public void Execute_StartStrategy() {
			mPlayQueue.StartPlayStrategy();
		}

		public bool CanExecute_StartStrategy() {
			return( mPlayQueue.CanStartPlayStrategy );
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

		public PlayStrategyItem PlayStrategy {
			get {
			    var strategy = mPlayQueue.PlayStrategy;

                return( mPlayStrategies.FirstOrDefault( item => item.StrategyId == strategy.StrategyId ));
			}
			set {
                var strategy = mPlayStrategyFactory.ProvidePlayStrategy( value.StrategyId );

                if( strategy != null ) {
    				IPlayStrategyParameters	parameters;

                    if( PromptForPlayStrategyItem( strategy, out parameters )) {
                        mPlayQueue.SetPlayStrategy( strategy.StrategyId, parameters );

                        var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					    if( configuration != null ) {
						    configuration.PlayStrategy = strategy.StrategyId;
						    configuration.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( parameters );

						    NoiseSystemConfiguration.Current.Save( configuration );
					    }
                    }
                }
			}
		}

		private bool PromptForPlayStrategyItem( IPlayStrategy strategy, out IPlayStrategyParameters parameters ) {
			var retValue = false;
			parameters = null;

			if( strategy.RequiresParameters ) {
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
