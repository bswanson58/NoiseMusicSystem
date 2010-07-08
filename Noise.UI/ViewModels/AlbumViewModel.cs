﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;
		private DbAlbum				mCurrentAlbum;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
			}
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value );  }
		}

		public void OnAlbumFocus( DbAlbum album ) {
			mCurrentAlbum = album;

			if( mCurrentAlbum != null ) {
				SupportInfo = mNoiseManager.DataProvider.GetAlbumSupportInfo( mCurrentAlbum );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] AlbumCover {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.AlbumCovers != null ) &&
				   ( SupportInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					var cover = ( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault();

					if( cover == null ) {
						cover = ( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault();
					}
					if( cover == null ) {
						cover = SupportInfo.AlbumCovers[0];
					}

					if( cover != null ) {
						retValue = cover.Image;
					}
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public IList<byte[]> AlbumArtwork {
			get {
				List<byte[]>	retValue;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Artwork != null )) {
					retValue = ( from DbArtwork artwork in SupportInfo.Artwork select artwork.Image ).ToList();
				}
				else {
					retValue = new List<byte[]>();
				}

				return( retValue );
			}
		}
	}
}
