using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : ViewModelBase {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private UiArtist					mCurrentArtist;
		private LinkNode					mArtistWebsite;
		private readonly Observer			mChangeObserver;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<LinkNode>				mSimilarArtists;
		private readonly ObservableCollectionEx<LinkNode>				mTopAlbums;
		private readonly ObservableCollectionEx<LinkNode>				mBandMembers;
		private readonly ObservableCollectionEx<DbDiscographyRelease>	mDiscography;

		public ArtistViewModel() {
			mSimilarArtists = new ObservableCollectionEx<LinkNode>();
			mTopAlbums = new ObservableCollectionEx<LinkNode>();
			mBandMembers = new ObservableCollectionEx<LinkNode>();
			mDiscography = new ObservableCollectionEx<DbDiscographyRelease>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveSupportInfo( args.Argument as UiArtist );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SupportInfo = result.Result as ArtistSupportInfo;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
				mEvents.GetEvent<Events.ArtistContentUpdated>().Subscribe( OnArtistInfoUpdate );
				mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
			}
		}

		public UiArtist Artist {
			get{ return( mCurrentArtist ); }
		}

		[DependsUpon( "Artist" )]
		public bool ArtistValid {
			get{ return( Artist != null ); }
		}

		private ArtistSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
				BeginInvoke( () => {
					mSimilarArtists.Clear();
					mTopAlbums.Clear();
					mBandMembers.Clear();
					mDiscography.Clear();

					if(( CurrentArtist != null ) &&
					   ( value != null )) {
						if(( value.SimilarArtist != null ) &&
						   ( value.SimilarArtist.Items.GetLength( 0 ) > 0 )) {
							mSimilarArtists.AddRange( from DbAssociatedItem artist in value.SimilarArtist.Items
														select artist.IsLinked ? new LinkNode( artist.Item, artist.AssociatedId, OnSimilarArtistClicked ) :
																				 new LinkNode( artist.Item ));
						}

						if(( value.TopAlbums != null ) &&
						   ( value.TopAlbums.Items.GetLength( 0 ) > 0 )) {
							mTopAlbums.AddRange( from DbAssociatedItem album in value.TopAlbums.Items 
												 select album.IsLinked ? new LinkNode( album.Item, album.AssociatedId, OnSimilarArtistClicked ) :
																		 new LinkNode( album.Item ));
						}

						if(( value.BandMembers != null ) &&
						   ( value.BandMembers.Items.GetLength( 0 ) > 0 )) {
							mBandMembers.AddRange( from DbAssociatedItem member in value.BandMembers.Items select new LinkNode( member.Item ));
						}

						using( var discoList = mNoiseManager.DataProvider.GetDiscography( CurrentArtist.DbId )) {
							mDiscography.SuspendNotification();
							mDiscography.AddRange( discoList.List );
							mDiscography.Sort( release => release.Year, ListSortDirection.Descending );
							mDiscography.ResumeNotification();
						}
					}

					Set( () => SupportInfo, value );
				});
			}
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist( null );

			Mapper.DynamicMap( dbArtist, retValue );
			retValue.DisplayGenre = mNoiseManager.TagManager.GetGenre( dbArtist.Genre );

			return( retValue );
		}

		private UiArtist CurrentArtist {
			get{ return( mCurrentArtist ); }
			set {
				if( mCurrentArtist != null ) {
					mChangeObserver.Release( mCurrentArtist );

					if(( value != null ) &&
					   ( mCurrentArtist.DbId != value.DbId )) {
						SupportInfo = null;
					}
				}

				if( value != null ) {
					mNoiseManager.DataProvider.UpdateArtistInfo( value.DbId );

					mCurrentArtist = TransformArtist( mNoiseManager.DataProvider.GetArtist( value.DbId ));
					mChangeObserver.Add( mCurrentArtist );

					if(!mBackgroundWorker.IsBusy ) {
						mBackgroundWorker.RunWorkerAsync( mCurrentArtist );
					}
					BeginInvoke( () => {
						RaisePropertyChanged( () => Artist );

						mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
						RaisePropertyChanged( () => ArtistWebsite );
					});
				}
				else {
					mCurrentArtist = null;
				}
			}
		}

		private void OnArtistInfoUpdate( DbArtist artist ) {
			if(( artist != null ) &&
			   ( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == artist.DbId )) {
				if(!mBackgroundWorker.IsBusy ) {
					mBackgroundWorker.RunWorkerAsync( CurrentArtist );
				}
			}
		}

		private ArtistSupportInfo RetrieveSupportInfo( UiArtist forArtist ) {
			return( mNoiseManager.DataProvider.GetArtistSupportInfo( forArtist.DbId ));
		}

		private void OnSimilarArtistClicked( long artistId ) {
			var artist = mNoiseManager.DataProvider.GetArtist( artistId  );

			if( artist != null ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
			if( artist != null ) {
				if( CurrentArtist != null ) {
					if( artist.DbId != CurrentArtist.DbId ) {
						CurrentArtist = TransformArtist( artist );
					}
				}
				else {
					CurrentArtist = TransformArtist( artist );
				}
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			if( CurrentArtist != null ) {
				if( album.Artist != CurrentArtist.DbId ) {
					CurrentArtist = TransformArtist( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
				}
			}
			else {
				CurrentArtist = TransformArtist( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
			}
		}

		private static void OnArtistChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UiArtist;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			var item = args.GetItem( mNoiseManager.DataProvider );

			if(( item is DbArtist ) &&
			   ( CurrentArtist != null ) &&
			   ( args.ItemId == CurrentArtist.DbId )) {
				CurrentArtist = TransformArtist( item as DbArtist );
			}
		}

		private void OnWebsiteRequested( long id ) {
			if(( CurrentArtist != null ) &&
			   (!string.IsNullOrWhiteSpace( CurrentArtist.Website ))) {
				mEvents.GetEvent<Events.WebsiteRequest>().Publish( CurrentArtist.Website );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.ArtistImage != null )) {
					retValue = SupportInfo.ArtistImage.Image;
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public string ArtistBio {
			get {
				var retValue = "";

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Biography != null )) {
					retValue = SupportInfo.Biography.Text;
				}

				return( retValue );
			}
		}

		public LinkNode ArtistWebsite {
			get{ return( mArtistWebsite ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> TopAlbums {
			get{ return( mTopAlbums ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> SimilarArtist {
			get { return( mSimilarArtists ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> BandMembers {
			get { return( mBandMembers ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<DbDiscographyRelease> Discography {
			get{ return( mDiscography ); }
		}
	}
}
