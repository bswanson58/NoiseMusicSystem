using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Noise.Core.Support;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ContentBuilder : IBackgroundTask, IRequireInitialization {
		private readonly IDataProvider	mDataProvider;
		private	List<DbArtist>			mArtistList;
		private IEnumerator<DbArtist>	mArtistEnum;

		public ContentBuilder( ILifecycleManager lifecycleManager, IDataProvider dataProvider ) {
			mDataProvider = dataProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_ContentBuilder" ); }
		}

		public void Initialize() {
			BuildArtistList();
			if( mArtistList.Count() > 0 ) {
				var seed = new Random( DateTime.Now.Millisecond );
				var random = seed.Next( mArtistList.Count() - 1 );

				while(( mArtistEnum.MoveNext()) &&
						( random > 0 )) {
					random--;
				}
			}
		}

		public void Shutdown() { }

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
	}
}
