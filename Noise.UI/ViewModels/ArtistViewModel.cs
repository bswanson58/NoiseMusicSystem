using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : ViewModelBase {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private DbArtist					mCurrentArtist;
		public	UserSettingsNotifier		UiEdit { get; private set; }
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
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveSupportInfo( args.Argument as DbArtist );
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

		public DbArtist UiDisplay {
			get{ return( mCurrentArtist ); }
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

						using( var discoList = mNoiseManager.DataProvider.GetDiscography( CurrentArtist )) {
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

		private DbArtist CurrentArtist {
			get{ return( mCurrentArtist ); }
			set {
				if(( mCurrentArtist != null ) &&
				   ( value != null ) &&
				   ( mCurrentArtist.DbId != value.DbId )) {
					SupportInfo = null;
				}

				mNoiseManager.DataProvider.UpdateArtistInfo( value );

				BeginInvoke( () => {
					mCurrentArtist = value != null ? mNoiseManager.DataProvider.GetArtist( value.DbId ) : null;

					if( mCurrentArtist != null ) {
						if( UiEdit != null ) {
							mChangeObserver.Release( UiEdit );
						}
						UiEdit = new UserSettingsNotifier( mCurrentArtist, null );
						mChangeObserver.Add( UiEdit );

						RaisePropertyChanged( () => UiDisplay );
						RaisePropertyChanged( () => UiEdit );

						mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
						RaisePropertyChanged( () => ArtistWebsite );

						if(!mBackgroundWorker.IsBusy ) {
							mBackgroundWorker.RunWorkerAsync( CurrentArtist );
						}
					}
				});
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

		private ArtistSupportInfo RetrieveSupportInfo( DbArtist forArtist ) {
			return( mNoiseManager.DataProvider.GetArtistSupportInfo( forArtist ));
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
						CurrentArtist = artist;
					}
				}
				else {
					CurrentArtist = artist;
				}
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			if( CurrentArtist != null ) {
				if( album.Artist != CurrentArtist.DbId ) {
					CurrentArtist = mNoiseManager.DataProvider.GetArtistForAlbum( album );
				}
			}
			else {
				CurrentArtist = mNoiseManager.DataProvider.GetArtistForAlbum( album );
			}
		}

		private void OnArtistChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					mNoiseManager.DataProvider.SetRating( mCurrentArtist, notifier.UiRating );
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					mNoiseManager.DataProvider.SetFavorite( mCurrentArtist, notifier.UiIsFavorite );
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if(( args.Item is DbArtist ) &&
			   ( CurrentArtist != null ) &&
			   ((args.Item as DbArtist).DbId == CurrentArtist.DbId )) {
				CurrentArtist = args.Item as DbArtist;
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
