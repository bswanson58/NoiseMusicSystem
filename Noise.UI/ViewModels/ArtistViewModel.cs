using System.Collections.Generic;
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

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
			}
		}

		private ArtistSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value );  }
		}

		public void OnArtistFocus( DbArtist artist ) {
			mCurrentArtist = artist;

			if( mCurrentArtist != null ) {
				SupportInfo = mNoiseManager.DataProvider.GetArtistSupportInfo( artist );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Biography != null )) {
					retValue = SupportInfo.Biography.ArtistImage;
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
					retValue = SupportInfo.Biography.Biography;
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
				retValue = SupportInfo.TopAlbums.TopItems;
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
					retValue = SupportInfo.SimilarArtist.SimilarItems;
				}

				return( retValue );
			}
		}
	}
}
