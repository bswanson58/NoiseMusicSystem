using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class PlayStrategyDialogModel : DialogModelBase {
		private readonly IArtistProvider			mArtistProvider;
		private readonly IGenreProvider             mGenreProvider;
		private readonly ITagProvider               mTagProvider;
		private readonly IPlayListProvider          mPlayListProvider;
		private readonly IPlayStrategyFactory       mPlayStrategyFactory;
		private readonly IExhaustedStrategyFactory  mExhaustedStrategyFactory;
		private readonly List<DbArtist>				mArtistList;
		private readonly List<DbGenre>				mArtistGenreList;
		private readonly List<DbPlayList>			mPlayLists; 
		private readonly List<DbTag>				mCategoryList;
        private readonly List<DbTag>                mUserTags;
		private IPlayStrategy						mSelectedPlayStrategy;
		private IPlayStrategyParameters				mPlayStrategyParameters;
        private ExhaustedStrategySpecification      mExhaustedStrategySpecification;
        private IStrategyDescription                mSelectedExhaustedStrategy;

		private	readonly BindableCollection<IStrategyDescription>	mExhaustedStrategies;
		private readonly BindableCollection<NameIdPair>				mExhaustedParameters; 
		private readonly BindableCollection<PlayStrategyItem>		mPlayStrategies;
		private readonly BindableCollection<NameIdPair>				mPlayParameters;

        public  BindableCollection<PlayStrategyItem>                PlayStrategyList => mPlayStrategies;
        public  BindableCollection<IStrategyDescription>            ExhaustedStrategyList => mExhaustedStrategies;

		public PlayStrategyDialogModel( IArtistProvider artistProvider, IGenreProvider genreProvider, ITagProvider tagProvider,
										IPlayListProvider playListProvider,
										IPlayStrategyFactory strategyFactory, IExhaustedStrategyFactory exhaustedFactory ) {
			mArtistProvider = artistProvider;
			mGenreProvider = genreProvider;
			mTagProvider = tagProvider;
			mPlayListProvider = playListProvider;
			mPlayStrategyFactory = strategyFactory;
			mExhaustedStrategyFactory = exhaustedFactory;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>( from strategy in mPlayStrategyFactory.AvailableStrategies orderby strategy.StrategyName
																		select new PlayStrategyItem( strategy.StrategyId, strategy.StrategyName ));

			mExhaustedStrategies = new BindableCollection<IStrategyDescription>( from strategy in mExhaustedStrategyFactory.ExhaustedStrategies
                                                                                 where strategy.StrategyType == eTrackPlayStrategy.Suggester orderby strategy.Name select strategy);

			mPlayParameters = new BindableCollection<NameIdPair>();
			mExhaustedParameters = new BindableCollection<NameIdPair>();

			mArtistList = new List<DbArtist>();
			mCategoryList = new List<DbTag>();
			mArtistGenreList = new List<DbGenre>();
			mPlayLists = new List<DbPlayList>();
            mUserTags = new List<DbTag>();
		}

		public bool DeletePlayedTracks {
			get { return( Get( () => DeletePlayedTracks )); }
			set { Set( () => DeletePlayedTracks, value ); }
		}

		public ePlayStrategy PlayStrategy {
			get => ( mSelectedPlayStrategy.StrategyId );
            set {
				mSelectedPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( value );

				SelectedPlayStrategy = mPlayStrategies.FirstOrDefault( strategy => strategy.StrategyId == mSelectedPlayStrategy.StrategyId );
			}
		}

		public IPlayStrategyParameters PlayStrategyParameter {
			get => ( mPlayStrategyParameters );
            set {
				mPlayStrategyParameters = value;

				if( mPlayStrategyParameters is PlayStrategyParameterDbId ) {
					var dbParms = mPlayStrategyParameters as PlayStrategyParameterDbId;

					SelectedPlayParameter = mPlayParameters.FirstOrDefault( parameter => parameter.Id == dbParms.DbItemId );
				}
			}
		}

        public ExhaustedStrategySpecification ExhaustedStrategySpecification {
            get => mExhaustedStrategySpecification;
            set {
                mExhaustedStrategySpecification = value;

                if(( mExhaustedStrategySpecification != null ) &&
                   ( mExhaustedStrategySpecification.TrackSuggesters.Any())) {
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
						if( mExhaustedStrategySpecification.SuggesterParameter == Constants.cDatabaseNullOid ) {
							retValue = false;
						}
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
					mPlayStrategyParameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayArtist ) { DbItemId = SelectedPlayParameter.Id };
				}
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
                mExhaustedStrategySpecification.SetPrimarySuggester( mSelectedExhaustedStrategy.Identifier );

				if( mSelectedExhaustedStrategy != null ) {
					SetupExhaustedParameters( mSelectedExhaustedStrategy, mExhaustedParameters );
				}
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

                ExhaustedStrategySpecification.SuggesterParameter = SelectedExhaustedParameter?.Id ?? Constants.cDatabaseNullOid;
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

					case eTrackPlayHandlers.PlayArtistGenre:
					case eTrackPlayHandlers.PlayGenre:
						FillCollectionWithGenres( collection );
						break;

					case eTrackPlayHandlers.PlayCategory:
						FillCollectionWithCategories( collection );
						break;

					case eTrackPlayHandlers.PlayList:
						FillCollectionWithPlayLists( collection );
						break;

                    case eTrackPlayHandlers.PlayUserTags:
                        FillCollectionWithUserTags( collection );
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

		private void FillCollectionWithCategories( BindableCollection<NameIdPair> collection ) {
			if( !mCategoryList.Any() ) {
				using( var categoryList = mTagProvider.GetTagList( eTagGroup.User )) {
					mCategoryList.AddRange( from DbTag tag in categoryList.List orderby tag.Name ascending  select tag );
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var category in mCategoryList ) {
				collection.Add( new NameIdPair( category.DbId, category.Name ));
			}

			collection.IsNotifying = true;
			collection.Refresh();
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

		private void FillCollectionWithPlayLists( BindableCollection<NameIdPair> collection ) {
			if(!mPlayLists.Any()) {
				using( var playLists = mPlayListProvider.GetPlayLists() ) {
					mPlayLists.AddRange( from playList in playLists.List orderby playList.Name select playList);
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var playtList in mPlayLists ) {
				collection.Add( new NameIdPair( playtList.DbId, playtList.Name ));
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
	}
}
