using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private DbArtist				mCurrentArtist;
		private BackgroundWorker		mBackgroundWorker;
		private readonly ObservableCollectionEx<LinkNode>	mSimilarArtists;
		private readonly ObservableCollectionEx<LinkNode>	mTopAlbums;
		private readonly ObservableCollectionEx<LinkNode>	mBandMembers;

		public ArtistViewModel() {
			mSimilarArtists = new ObservableCollectionEx<LinkNode>();
			mTopAlbums = new ObservableCollectionEx<LinkNode>();
			mBandMembers = new ObservableCollectionEx<LinkNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mBackgroundWorker = new BackgroundWorker();
				mBackgroundWorker.DoWork += ( o, args ) => args.Result = mNoiseManager.DataProvider.GetArtistSupportInfo( args.Argument as DbArtist );
				mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SupportInfo = result.Result as ArtistSupportInfo;

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
				mEvents.GetEvent<Events.ArtistContentUpdated>().Subscribe( OnArtistUpdate );
			}
		}

		private ArtistSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
				Invoke( () => {
					mSimilarArtists.Clear();
					mTopAlbums.Clear();
					mBandMembers.Clear();

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

					Set( () => SupportInfo, value );
				});
			}
		}

		private void OnSimilarArtistClicked( long artistId ) {
			var artist = mNoiseManager.DataProvider.GetArtist( artistId  );

			mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
		}

		public void OnArtistFocus( DbArtist artist ) {
			mCurrentArtist = artist;
			mNoiseManager.DataProvider.UpdateArtistInfo( artist );

			OnArtistUpdate( artist );
		}

		public void OnArtistUpdate( DbArtist artist ) {
			if(( artist != null ) &&
			   ( mCurrentArtist != null ) &&
			   ( string.Equals( mCurrentArtist.Name, artist.Name ))) {
				if(!mBackgroundWorker.IsBusy ) {
					mBackgroundWorker.RunWorkerAsync( artist );
				}
			}
		}

		public void OnAlbumFocus( DbAlbum album ) {
			if( mCurrentArtist != null ) {
				if( album.Artist != mNoiseManager.DataProvider.GetObjectIdentifier( mCurrentArtist )) {
					OnArtistFocus( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
				}
			}
			else {
				OnArtistFocus( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
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
	}
}
