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
		private readonly INoiseManager		mNoiseManager;
		private readonly IDatabaseManager	mDatabase;
		private	IEnumerator<DbArtist>		mArtistEnum;

		public BackgroundContentExplorer( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public bool Initialize() {
			mArtistEnum = ( from DbArtist artist in mDatabase.Database select artist ).AsEnumerable().GetEnumerator();

			return( true );
		}

		public bool BuildContent() {
			bool	retValue = false;

			if( mArtistEnum.MoveNext()) {
				mNoiseManager.DataProvider.UpdateArtistInfo( mArtistEnum.Current );

				retValue = true;
			}

			return( retValue );
		}
	}
}
