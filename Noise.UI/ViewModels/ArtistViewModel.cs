using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;
		private DbArtist			mCurrentArtist;
		private BackgroundWorker	mBackgroundWorker;

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
			set{ Set( () => SupportInfo, value );  }
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
		public IList<string> TopAlbums {
			get{
				IList<string>	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.TopAlbums != null )) {
				retValue = SupportInfo.TopAlbums.Items;
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public IList<string> SimilarArtist {
			get {
				IList<string>	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.SimilarArtist != null )) {
					retValue = SupportInfo.SimilarArtist.Items;
				}

				return( retValue );
			}
		}
	}
}
