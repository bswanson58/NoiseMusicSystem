using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
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
		private readonly IInternetStreamProvider	mStreamProvider;
        private readonly IPlayStrategyFactory       mPlayStrategyFactory;
		private readonly IPlayExhaustedFactory		mPlayExhaustedFactory;
		private readonly List<DbArtist>				mArtistList;
		private readonly List<DbGenre>				mGenreList;
		private readonly List<DbInternetStream>		mStreamList;
 		private readonly List<DbPlayList>			mPlayLists; 
		private readonly List<DbTag>				mCategoryList; 
		private IPlayStrategy						mSelectedPlayStrategy;
		private IPlayStrategyParameters				mPlayStrategyParameters;
		private IPlayExhaustedStrategy				mSelectedExhaustedStrategy;
		private IPlayStrategyParameters				mPlayExhaustedParameters;

		private	readonly BindableCollection<ExhaustedStrategyItem>	mExhaustedStrategies;
 		private readonly BindableCollection<NameIdPair>				mExhaustedParameters; 
		private readonly BindableCollection<PlayStrategyItem>		mPlayStrategies;
		private readonly BindableCollection<NameIdPair>				mPlayParameters;

		public PlayStrategyDialogModel( IArtistProvider artistProvider, IGenreProvider genreProvider, ITagProvider tagProvider,
										IPlayListProvider playListProvider, IInternetStreamProvider streamProvider,
										IPlayStrategyFactory strategyFactory, IPlayExhaustedFactory exhaustedFactory ) {
			mArtistProvider = artistProvider;
			mGenreProvider = genreProvider;
			mTagProvider = tagProvider;
			mPlayListProvider = playListProvider;
			mStreamProvider = streamProvider;
			mPlayStrategyFactory = strategyFactory;
			mPlayExhaustedFactory = exhaustedFactory;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>( from strategy in mPlayStrategyFactory.AvailableStrategies orderby strategy.StrategyName
                                                                        select new PlayStrategyItem( strategy.StrategyId, strategy.StrategyName ));

			mExhaustedStrategies = new BindableCollection<ExhaustedStrategyItem>( from strategy in mPlayExhaustedFactory.AvailableStrategies orderby strategy.DisplayName
																					  select new ExhaustedStrategyItem( strategy.StrategyId, strategy.DisplayName ));

			mPlayParameters = new BindableCollection<NameIdPair>();
			mExhaustedParameters = new BindableCollection<NameIdPair>();

			mArtistList = new List<DbArtist>();
			mCategoryList = new List<DbTag>();
			mGenreList = new List<DbGenre>();
			mPlayLists = new List<DbPlayList>();
			mStreamList = new List<DbInternetStream>();
		}

		public ePlayStrategy PlayStrategy {
			get {  return( mSelectedPlayStrategy.StrategyId ); }
			set {
				mSelectedPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( value );

				SelectedPlayStrategy = mPlayStrategies.FirstOrDefault( strategy => strategy.StrategyId == mSelectedPlayStrategy.StrategyId );
			}
		}

		public IPlayStrategyParameters PlayStrategyParameter {
			get {  return( mPlayStrategyParameters ); }
			set {
				mPlayStrategyParameters = value;

				if( mPlayStrategyParameters is PlayStrategyParameterDbId ) {
					var dbParms = mPlayStrategyParameters as PlayStrategyParameterDbId;

					SelectedPlayParameter = mPlayParameters.FirstOrDefault( parameter => parameter.Id == dbParms.DbItemId );
				}
			}
		}

		public ePlayExhaustedStrategy ExhaustedStrategy {
			get {  return( mSelectedExhaustedStrategy.StrategyId); }
			set {
				mSelectedExhaustedStrategy = mPlayExhaustedFactory.ProvideExhaustedStrategy( value );

				SelectedExhaustedStrategy = mExhaustedStrategies.FirstOrDefault( strategy => strategy.Strategy == mSelectedExhaustedStrategy.StrategyId );
			}
		}

		public IPlayStrategyParameters ExhaustedStrategyParameter {
			get {  return( mPlayExhaustedParameters ); }
			set {
				mPlayExhaustedParameters = value;

				if( mPlayExhaustedParameters is PlayStrategyParameterDbId ) {
					var dbParams = mPlayExhaustedParameters as PlayStrategyParameterDbId;

					SelectedExhaustedParameter = mExhaustedParameters.FirstOrDefault( parameter => parameter.Id == dbParams.DbItemId );
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
						if( ExhaustedStrategyParameter == null ) {
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

		public BindableCollection<PlayStrategyItem> PlayStrategyList {
			get{ return( mPlayStrategies ); }
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

		public BindableCollection<NameIdPair> PlayParameterList {
			get {  return( mPlayParameters ); }
		}

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

        public BindableCollection<ExhaustedStrategyItem> ExhaustedStrategyList {
			get{ return( mExhaustedStrategies ); }
		}

		public ExhaustedStrategyItem SelectedExhaustedStrategy {
			get {  return( Get( () => SelectedExhaustedStrategy )); }
			set {
				Set( () => SelectedExhaustedStrategy, value );

				mSelectedExhaustedStrategy = mPlayExhaustedFactory.ProvideExhaustedStrategy( value.Strategy );

				if( mSelectedExhaustedStrategy != null ) {
					SetupExhaustedParameters( mSelectedExhaustedStrategy, mExhaustedParameters );
				}
			}
		}

		public BindableCollection<NameIdPair> ExhaustedParameterList {
			get {  return( mExhaustedParameters ); }
		}

		public NameIdPair SelectedExhaustedParameter {
			get { return( Get( () => SelectedExhaustedParameter )); }
			set {
				Set( () => SelectedExhaustedParameter, value );

				mPlayExhaustedParameters = null;

				if( value != null ) {
					mPlayExhaustedParameters = new PlayStrategyParameterDbId( SelectedExhaustedStrategy.Strategy ) { DbItemId = SelectedExhaustedParameter.Id };
				}
			}
		}

		public bool ExhaustedParameterRequired {
			get {  return( Get( () => ExhaustedParameterRequired )); }
			set {  Set( () => ExhaustedParameterRequired, value ); }
		}

		private void SetupPlayParameters( IPlayStrategy strategy, BindableCollection<NameIdPair> collection ) {
			PlayParameterRequired = strategy.RequiresParameters;
			PlayParameterName = strategy.ParameterName;

			if( strategy.RequiresParameters ) {
				switch( strategy.StrategyId ) {
					case ePlayStrategy.FeaturedArtists:
						FillCollectionWithArtists( collection );
						break;
				}
			}
		}

		private void SetupExhaustedParameters( IPlayExhaustedStrategy strategy, BindableCollection<NameIdPair> collection ) {
			ExhaustedParameterRequired = strategy.RequiresParameters;

			if( strategy.RequiresParameters ) {
				switch( strategy.StrategyId ) {
					case ePlayExhaustedStrategy.PlayArtist:
						FillCollectionWithArtists( collection );
						break;

					case ePlayExhaustedStrategy.PlayArtistGenre:
					case ePlayExhaustedStrategy.PlayGenre:
						FillCollectionWithGenres( collection );
						break;

					case ePlayExhaustedStrategy.PlayCategory:
						FillCollectionWithCategories( collection );
						break;

					case ePlayExhaustedStrategy.PlayStream:
						FillCollectionWithStreams( collection );
						break;

					case ePlayExhaustedStrategy.PlayList:
						FillCollectionWithPlayLists( collection );
						break;
				}
			}
		}

		private void FillCollectionWithArtists( BindableCollection<NameIdPair> collection ) {
			if(!mArtistList.Any()) {
				using( var artistList = mArtistProvider.GetArtistList()) {
					mArtistList.AddRange( from artist in artistList.List orderby artist.Name select artist );
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var artist in mArtistList ) {
				collection.Add( new NameIdPair( artist.DbId, artist.Name ));
			}

			collection.IsNotifying = true;
			collection.Refresh();
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
			if(!mGenreList.Any()) {
				using( var genreList = mGenreProvider.GetGenreList()) {
					mGenreList.AddRange( from genre in genreList.List orderby genre.Name ascending  select genre );
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var genre in mGenreList ) {
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

		private void FillCollectionWithStreams( BindableCollection<NameIdPair> collection ) {
			if(!mStreamList.Any()) {
				using( var streamList = mStreamProvider.GetStreamList() ) {
					mStreamList.AddRange( from stream in streamList.List orderby stream.Name select stream );
				}
			}

			collection.IsNotifying = false;
			collection.Clear();

			foreach( var stream in mStreamList ) {
				collection.Add( new NameIdPair( stream.DbId, stream.Name ));
			}

			collection.IsNotifying = true;
			collection.Refresh();
		}

	}
}
