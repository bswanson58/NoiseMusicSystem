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
	internal class DecadeTagBuilder : IBackgroundTask,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string						cDecadeTagBuilderId		= "ComponentId_TagBuilder";

		private readonly IEventAggregator			mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;
		private readonly ITimestampProvider			mTimestampProvider;
		private readonly ITagManager				mTagManager;
		private List<long>							mArtistList;
		private IEnumerator<long>					mArtistEnum;
		private long								mLastScanTicks;
		private	long								mStartScanTicks;

		public DecadeTagBuilder( IEventAggregator eventAggregator, ITagAssociationProvider tagAssociationProvider, ITimestampProvider timestampProvider,
								 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mTimestampProvider = timestampProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTagManager = tagManager;

			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return( "Task_DecadeTagBuilder" ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
		}

		private void InitializeLists() {
			try {
				mLastScanTicks = mTimestampProvider.GetTimestamp( cDecadeTagBuilderId );
				mStartScanTicks = DateTime.Now.Ticks;

				using( var artistList = mArtistProvider.GetArtistList()) {
					mArtistList = new List<long>( from DbArtist artist in artistList.List select artist.DbId );
				}
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}
		}

		private long NextArtist() {
			if(!mArtistEnum.MoveNext()) {
				mTimestampProvider.SetTimestamp( cDecadeTagBuilderId, mStartScanTicks );

				InitializeLists();
				mArtistEnum.MoveNext();
			}

			return( mArtistEnum.Current );
		}

		public void ExecuteTask() {
			try {
				var artistId = NextArtist();

				if( artistId != 0 ) {
					var artist = mArtistProvider.GetArtist( artistId );

					if(( artist != null ) &&
					   ( artist.LastChangeTicks > mLastScanTicks )) {
						using( var tagList = mTagAssociationProvider.GetArtistTagList( artistId, eTagGroup.Decade )) {
							foreach( var tag in tagList.List ) {
								mTagAssociationProvider.RemoveAssociation( tag.DbId );
							}

							using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
								var decadeTagList = mTagManager.DecadeTagList.OrderBy( tag => tag.StartYear );

								foreach( var album in albumList.List ) {
									var publishedYear = album.PublishedYear;
									var decadeTag = decadeTagList.FirstOrDefault(decade => publishedYear >= decade.StartYear && publishedYear <= decade.EndYear);

									if( decadeTag != null ) {
										var associationTag = new DbTagAssociation( eTagGroup.Decade, decadeTag.DbId, artistId, album.DbId );

										mTagAssociationProvider.AddAssociation( associationTag );
									}
								}

								NoiseLogger.Current.LogMessage( string.Format( "Built decade tag associations for: {0}", artist.Name ));
								mEventAggregator.Publish( new Events.StatusEvent( string.Format( "Built decade tag associations for: {0}", artist.Name )));
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DecadeTagBuilder:Task ", ex );
			}
		}
	}
}
