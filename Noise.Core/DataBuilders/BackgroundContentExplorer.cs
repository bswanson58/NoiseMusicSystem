using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Quartz;

namespace Noise.Core.DataBuilders {
	internal class BackgroundContentExplorerJob : IJob {
		public void Execute( JobExecutionContext context ) {
			if( context != null ) {
				var	explorer = context.Trigger.JobDataMap[NoiseManager.cBackgroundContentExplorer] as BackgroundContentExplorer;

				if( explorer != null ) {
					explorer.BuildContent();
				}
			}
		}
	}

	internal class BackgroundContentExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDataProvider		mDataProvider;
		private	IEnumerator<DbArtist>		mArtistEnum;

		public BackgroundContentExplorer( IUnityContainer container ) {
			mContainer = container;
			mDataProvider = mContainer.Resolve<IDataProvider>();
		}

		public bool Initialize() {
			if( mDataProvider.Initialize()) {
				var	list = mDataProvider.GetArtistList();
				var seed = new Random( DateTime.Now.Millisecond );
				var random = seed.Next( list.Count() - 1 );

				mArtistEnum =  list.Skip( random ).AsEnumerable().GetEnumerator();
			}

			return( true );
		}

		public bool BuildContent() {
			bool	retValue = false;

			if( mArtistEnum.MoveNext()) {
				mDataProvider.UpdateArtistInfo( mArtistEnum.Current );

				retValue = true;
			}

			return( retValue );
		}
	}
}
