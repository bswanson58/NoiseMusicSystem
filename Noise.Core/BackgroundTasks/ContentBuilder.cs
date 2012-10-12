using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ContentBuilder : IBackgroundTask,
								  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IArtistProvider	mArtistProvider;
		private	List<DbArtist>				mArtistList;
		private IEnumerator<DbArtist>		mArtistEnum;

		public ContentBuilder( IEventAggregator eventAggregator, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;

			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return( "Task_ContentBuilder" ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
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

		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
		}

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
