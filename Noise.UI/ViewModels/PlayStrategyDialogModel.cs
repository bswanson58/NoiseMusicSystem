﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayStrategyDialogModel : AutomaticPropertyBase, IDialogAware {
		private readonly IArtistProvider			mArtistProvider;
		private readonly IGenreProvider             mGenreProvider;
		private readonly ITagProvider               mTagProvider;
		private readonly IPlayQueue					mPlayQueue;
		private readonly IPlayStrategyFactory       mPlayStrategyFactory;
		private readonly IExhaustedStrategyFactory  mExhaustedStrategyFactory;
		private readonly List<DbArtist>				mArtistList;
		private readonly List<DbGenre>				mArtistGenreList;
        private readonly List<DbTag>                mUserTags;
		private IPlayStrategy						mSelectedPlayStrategy;
		private IPlayStrategyParameters				mPlayStrategyParameters;
        private ExhaustedStrategySpecification      mExhaustedStrategySpecification;
        private IStrategyDescription                mSelectedExhaustedStrategy;

        private readonly BindableCollection<NameIdPair>			mExhaustedParameters; 
		private readonly BindableCollection<PlayStrategyItem>	mPlayStrategies;
		private readonly BindableCollection<NameIdPair>			mPlayParameters;

        public  BindableCollection<PlayStrategyItem>            PlayStrategyList => mPlayStrategies;
        public  BindableCollection<IStrategyDescription>        ExhaustedStrategyList { get; }
        public  BindableCollection<UiStrategyDescription>       DisqualifierList { get; }
        public  BindableCollection<UiStrategyDescription>       BonusStrategyList { get; }

        public  string											Title { get; }
        public  DelegateCommand									Ok { get; }
        public  DelegateCommand									Cancel { get; }
        public  event Action<IDialogResult>						RequestClose;

        public PlayStrategyDialogModel( IArtistProvider artistProvider, IGenreProvider genreProvider, ITagProvider tagProvider, IPlayQueue playQueue,
										IPlayStrategyFactory strategyFactory, IExhaustedStrategyFactory exhaustedFactory ) {
			mArtistProvider = artistProvider;
			mGenreProvider = genreProvider;
			mTagProvider = tagProvider;
			mPlayQueue = playQueue;
			mPlayStrategyFactory = strategyFactory;
			mExhaustedStrategyFactory = exhaustedFactory;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>( from strategy in mPlayStrategyFactory.AvailableStrategies orderby strategy.StrategyName
																		select new PlayStrategyItem( strategy.StrategyId, strategy.StrategyName ));

			ExhaustedStrategyList = new BindableCollection<IStrategyDescription>( from strategy in mExhaustedStrategyFactory.ExhaustedStrategies
                                                                                 where strategy.StrategyType == eTrackPlayStrategy.Suggester orderby strategy.Name select strategy);
            DisqualifierList = new BindableCollection<UiStrategyDescription>( from strategy in mExhaustedStrategyFactory.ExhaustedStrategies
                                                                           where strategy.StrategyType == eTrackPlayStrategy.Disqualifier orderby strategy.Name 
                                                                           select new UiStrategyDescription( strategy ));
            BonusStrategyList = new BindableCollection<UiStrategyDescription>( from strategy in mExhaustedStrategyFactory.ExhaustedStrategies
                                                                             where strategy.StrategyType == eTrackPlayStrategy.BonusSuggester orderby strategy.Name 
                                                                             select new UiStrategyDescription( strategy));

			mPlayParameters = new BindableCollection<NameIdPair>();
			mExhaustedParameters = new BindableCollection<NameIdPair>();

			mArtistList = new List<DbArtist>();
			mArtistGenreList = new List<DbGenre>();
            mUserTags = new List<DbTag>();

			Ok = new DelegateCommand( OnOk, () => IsConfigurationValid );
			Cancel = new DelegateCommand( OnCancel );
			Title = "Play Strategy Configuration";
		}

        public void OnDialogOpened( IDialogParameters parameters ) {
            PlayStrategy = mPlayQueue.PlayStrategy.StrategyId;
            PlayStrategyParameter = mPlayQueue.PlayStrategy.Parameters;
            ExhaustedStrategySpecification = mPlayQueue.ExhaustedPlayStrategy;
            DeletePlayedTracks = mPlayQueue.DeletedPlayedTracks;
        }

        public bool DeletePlayedTracks {
			get { return( Get( () => DeletePlayedTracks )); }
			set { Set( () => DeletePlayedTracks, value ); }
		}

		public ePlayStrategy PlayStrategy {
			get => mSelectedPlayStrategy.StrategyId;
            set {
				mSelectedPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( value );

				SelectedPlayStrategy = mPlayStrategies.FirstOrDefault( strategy => strategy.StrategyId == mSelectedPlayStrategy.StrategyId );

				RaisePropertyChanged( () => PlayStrategy );
				Ok.RaiseCanExecuteChanged();
			}
		}

		public IPlayStrategyParameters PlayStrategyParameter {
			get => mPlayStrategyParameters;
            set {
				mPlayStrategyParameters = value;

				if( mPlayStrategyParameters is PlayStrategyParameterDbId dbParms ) {
                    SelectedPlayParameter = mPlayParameters.FirstOrDefault( parameter => parameter.Id == dbParms.DbItemId );
				}

				RaisePropertyChanged( () => PlayStrategyParameter );
                Ok.RaiseCanExecuteChanged();
			}
		}

        public ExhaustedStrategySpecification ExhaustedStrategySpecification {
            get {
                UpdateStrategySpecification();

                return mExhaustedStrategySpecification;
            }
            set {
                mExhaustedStrategySpecification = value;

                LoadStrategySpecification();

				RaisePropertyChanged( () => ExhaustedStrategySpecification );
                Ok.RaiseCanExecuteChanged();
            }
        }

        private void UpdateStrategySpecification() {
            mExhaustedStrategySpecification.TrackDisqualifiers.Clear();
            DisqualifierList.ForEach( d => {
                if( d.IsSelected ) {
                    mExhaustedStrategySpecification.TrackDisqualifiers.Add( d.StrategyId );
                }
            });

            mExhaustedStrategySpecification.TrackBonusSuggesters.Clear();
            BonusStrategyList.ForEach( h => {
                if( h.IsSelected ) {
                    mExhaustedStrategySpecification.TrackBonusSuggesters.Add( h.StrategyId );
                }
            });

            mExhaustedStrategySpecification.SetPrimarySuggester( mSelectedExhaustedStrategy.Identifier );
            mExhaustedStrategySpecification.SuggesterParameter = SelectedExhaustedParameter?.Id ?? Constants.cDatabaseNullOid;
        }

        private void LoadStrategySpecification() {
            if( mExhaustedStrategySpecification != null ) {
                DisqualifierList.ForEach( d => d.IsSelected = mExhaustedStrategySpecification.TrackDisqualifiers.Contains( d.StrategyId ));
                BonusStrategyList.ForEach( b => b.IsSelected = mExhaustedStrategySpecification.TrackBonusSuggesters.Contains( b.StrategyId ));

                if( mExhaustedStrategySpecification.TrackSuggesters.Any()) {
                    var suggester = mExhaustedStrategySpecification.TrackSuggesters.First();

                    SelectedExhaustedStrategy = mExhaustedStrategyFactory.ExhaustedStrategies.FirstOrDefault( s => s.Identifier.Equals( suggester ));
                    SelectedExhaustedParameter = ExhaustedParameterList.FirstOrDefault( p => p.Id.Equals( mExhaustedStrategySpecification.SuggesterParameter ));
                }
            }
        }

		public bool IsConfigurationValid {
			get {
				var retValue = true;

				if( mSelectedPlayStrategy != null ) {
					if( mSelectedPlayStrategy.RequiresParameters ) {
						if( PlayStrategyParameter == null ) {
							retValue = false;
						}
					}
				}
				else {
					retValue = false;
				}

				if( mSelectedExhaustedStrategy != null ) {
					if( mSelectedExhaustedStrategy.RequiresParameters ) {
						retValue &= SelectedExhaustedParameter != null;
					}
				}
				else {
					retValue = false;
				}

				return( retValue );
			}
		}

        public PlayStrategyItem SelectedPlayStrategy {
			get {  return( Get( () => SelectedPlayStrategy )); }
			set {
				Set( () => SelectedPlayStrategy, value );

				mSelectedPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( value.StrategyId );

				if( mSelectedPlayStrategy != null ) {
					SetupPlayParameters( mSelectedPlayStrategy, mPlayParameters );
				}

				RaisePropertyChanged( () => SelectedPlayStrategy );
				Ok.RaiseCanExecuteChanged();
			}
		}

		public string PlayStrategyDescription {
			get {  return( Get( () => PlayStrategyDescription )); }
			set {  Set( () => PlayStrategyDescription, value ); }
		}

		public BindableCollection<NameIdPair> PlayParameterList => mPlayParameters;

        public NameIdPair SelectedPlayParameter {
			get { return( Get( () => SelectedPlayParameter )); }
			set {
				Set( () => SelectedPlayParameter, value );

				mPlayStrategyParameters = null;
				if( value != null ) {
					mPlayStrategyParameters = new PlayStrategyParameterDbId( eTrackPlayHandlers.PlayArtist ) { DbItemId = SelectedPlayParameter.Id };
				}

				RaisePropertyChanged( () => SelectedPlayParameter );
				Ok.RaiseCanExecuteChanged();
			}
		}

		public string PlayParameterName {
			get {  return( Get( () => PlayParameterName )); }
			set {  Set( () => PlayParameterName, value ); }
		}

		public bool PlayParameterRequired {
			get { return( Get( () => PlayParameterRequired )); }
			set { Set( () => PlayParameterRequired, value ); }
		}

        public IStrategyDescription SelectedExhaustedStrategy {
			get {  return( Get( () => SelectedExhaustedStrategy )); }
			set {
				Set( () => SelectedExhaustedStrategy, value );

				mSelectedExhaustedStrategy = value;

				if( mSelectedExhaustedStrategy != null ) {
					SetupExhaustedParameters( mSelectedExhaustedStrategy, mExhaustedParameters );
				}

				RaisePropertyChanged( () => SelectedExhaustedStrategy );
				Ok.RaiseCanExecuteChanged();
			}
		}

		public string ExhaustedStrategyDescription {
			get {  return( Get( () => ExhaustedStrategyDescription )); }
			set {  Set( () => ExhaustedStrategyDescription, value ); }
		}

		public BindableCollection<NameIdPair> ExhaustedParameterList => mExhaustedParameters;

        public NameIdPair SelectedExhaustedParameter {
			get { return( Get( () => SelectedExhaustedParameter )); }
			set {
                Set( () => SelectedExhaustedParameter, value );

				Ok.RaiseCanExecuteChanged();
            }
		}

		public string ExhaustedParameterName {
			get {  return( Get( () => ExhaustedParameterName )); }
			set {  Set( () => ExhaustedParameterName, value ); }
		}

		public bool ExhaustedParameterRequired {
			get {  return( Get( () => ExhaustedParameterRequired )); }
			set {  Set( () => ExhaustedParameterRequired, value ); }
		}

		private void SetupPlayParameters( IPlayStrategy strategy, BindableCollection<NameIdPair> collection ) {
			PlayParameterRequired = strategy.RequiresParameters;
			PlayParameterName = strategy.ParameterName;
			PlayStrategyDescription = strategy.StrategyDescription;

			if( strategy.RequiresParameters ) {
				switch( strategy.StrategyId ) {
					case ePlayStrategy.FeaturedArtists:
						FillCollectionWithArtists( collection );
						break;
				}
			}
		}

		private void SetupExhaustedParameters( IStrategyDescription strategy, BindableCollection<NameIdPair> collection ) {
			ExhaustedParameterRequired = strategy.RequiresParameters;
			ExhaustedParameterName = strategy.Name;
			ExhaustedStrategyDescription = strategy.Description;

			if( strategy.RequiresParameters ) {
				switch( strategy.Identifier ) {
					case eTrackPlayHandlers.PlayArtist:
						FillCollectionWithArtists( collection );
						break;

					case eTrackPlayHandlers.PlayGenre:
						FillCollectionWithGenres( collection );
						break;

                    case eTrackPlayHandlers.PlayUserTags:
                        FillCollectionWithUserTags( collection );
                        break;

                    case eTrackPlayHandlers.RatedTracks:
                        FillCollectionWithRatings( collection );
                        break;
				}
			}
		}

		private void FillCollectionWithArtists( BindableCollection<NameIdPair> collection ) {
			FillArtistList();

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var artist in mArtistList ) {
				collection.Add( new NameIdPair( artist.DbId, artist.Name ));
			}

			collection.IsNotifying = true;
			collection.Refresh();
		}

		private void FillArtistList() {
			if(!mArtistList.Any()) {
				using( var artistList = mArtistProvider.GetArtistList()) {
					mArtistList.AddRange( from artist in artistList.List orderby artist.Name select artist );
				}
			}
		}

		private void FillCollectionWithGenres( BindableCollection<NameIdPair> collection ) {
			FillArtistList();

			if(!mArtistGenreList.Any()) {
				using( var genreList = mGenreProvider.GetGenreList()) {
					var allGenres = new List<DbGenre>( from genre in genreList.List orderby genre.Name ascending select genre );
					
					mArtistGenreList.AddRange( allGenres.Join( mArtistList, genre => genre.DbId, artist => artist.Genre, ( genre, artist ) => genre ).Distinct());
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var genre in mArtistGenreList ) {
				collection.Add( new NameIdPair( genre.DbId, genre.Name ));
			}

			collection.IsNotifying = true;
			collection.Refresh();
		}

        private void FillCollectionWithUserTags( BindableCollection<NameIdPair> collection ) {
            if(!mUserTags.Any()) {
                using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
                    mUserTags.AddRange( from DbTag tag in tagList.List orderby tag.Name select tag );
                }
            }

            collection.IsNotifying = false;
            collection.Clear();

            foreach( var tag in mUserTags ) {
                collection.Add( new NameIdPair( tag.DbId, tag.Name ));
            }

            collection.IsNotifying = true;
            collection.Refresh();
        }

        private void FillCollectionWithRatings( BindableCollection<NameIdPair> collection ) {
            collection.IsNotifying = false;
            collection.Clear();

            collection.Add( new NameIdPair( 5, "5 Stars or greater" ));
            collection.Add( new NameIdPair( 4, "4 Stars or greater" ));
            collection.Add( new NameIdPair( 3, "3 Stars or greater" ));
            collection.Add( new NameIdPair( 2, "2 Stars or greater" ));
            collection.Add( new NameIdPair( 1, "1 Star or greater" ));

            collection.IsNotifying = true;
            collection.Refresh();
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
			if( IsConfigurationValid ) {
                mPlayQueue.ExhaustedPlayStrategy = ExhaustedStrategySpecification;
                mPlayQueue.DeletedPlayedTracks = DeletePlayedTracks;
                mPlayQueue.SetPlayStrategy( PlayStrategy, PlayStrategyParameter );

                RaiseRequestClose( new DialogResult( ButtonResult.OK ));
            }
			else {
                RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
            }
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
	}
}
