using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ContentBuilder : IBackgroundTask, IRequireInitialization {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IArtistProvider	mArtistProvider;
		private	List<DbArtist>				mArtistList;
		private IEnumerator<DbArtist>		mArtistEnum;

		public ContentBuilder( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_ContentBuilder" ); }
		}

		public void Initialize() {
			BuildArtistList();
			if( mArtistList.Any()) {
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
				mEventAggregator.Publish( new Events.ArtistContentRequest( artist.DbId ));
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
			using( var list = mArtistProvider.GetArtistList()) {
				mArtistList = new List<DbArtist>( list.List );
				mArtistEnum = mArtistList.GetEnumerator();
			}
		}
	}
}
