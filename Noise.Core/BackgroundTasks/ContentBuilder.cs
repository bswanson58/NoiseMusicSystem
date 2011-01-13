using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ContentBuilder : IBackgroundTask {
		private IUnityContainer			mContainer;
		private IDataProvider			mDataProvider;
		private	List<DbArtist>			mArtistList;
		private IEnumerator<DbArtist>	mArtistEnum;

		public string TaskId {
			get { return( "Task_ContentBuilder" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			var retValue = false;

			mContainer = container;
			mDataProvider = mContainer.Resolve<IDataProvider>();

			BuildArtistList();
			if( mArtistList.Count() > 0 ) {
				var seed = new Random( DateTime.Now.Millisecond );
				var random = seed.Next( mArtistList.Count() - 1 );

				while(( mArtistEnum.MoveNext()) &&
						( random > 0 )) {
					random--;
				}

				retValue = true;
			}

			return( retValue );
		}

		public void ExecuteTask() {
			var artist = NextArtist();

			if( artist != null ) {
				mDataProvider.UpdateArtistInfo( artist.DbId );
			}
		}

		private DbArtist NextArtist() {
			if(!mArtistEnum.MoveNext()) {
				BuildArtistList();

				mArtistEnum.MoveNext();
			}

			return( mArtistEnum.Current );
		}

		private void BuildArtistList() {
			using( var list = mDataProvider.GetArtistList()) {
				mArtistList = new List<DbArtist>( list.List );
				mArtistEnum = mArtistList.GetEnumerator();
			}
		}

		public void Shutdown() {
		}
	}
}
