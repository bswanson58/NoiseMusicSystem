using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
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
			var retValue = false;
			var	list = mDataProvider.GetArtistList();

			if( list.List.Count() > 0 ) {
				var seed = new Random( DateTime.Now.Millisecond );
				var random = seed.Next( list.List.Count() - 1 );

				mArtistEnum =  list.List.Skip( random ).AsEnumerable().GetEnumerator();

				retValue = true;
			}

			return( retValue );
		}

		public bool BuildContent() {
			bool	retValue = false;

			if(( mArtistEnum != null ) &&
			   ( mArtistEnum.MoveNext())) {
				mDataProvider.UpdateArtistInfo( mArtistEnum.Current );

				retValue = true;
			}

			return( retValue );
		}
	}
}
