using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	internal class DecadeTagBuilder : IBackgroundTask, IRequireInitialization {
		private const string						cDecadeTagBuilderId		= "ComponentId_TagBuilder";

		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;
		private readonly ITimestampProvider			mTimestampProvider;
		private readonly ITagManager				mTagManager;
		private List<long>							mArtistList;
		private IEnumerator<long>					mArtistEnum;
		private long								mLastScanTicks;
		private	long								mStartScanTicks;

		public DecadeTagBuilder( ILifecycleManager lifecycleManager, ITagAssociationProvider tagAssociationProvider, ITimestampProvider timestampProvider,
								 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagManager tagManager ) {
			mTimestampProvider = timestampProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTagManager = tagManager;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_DiscographyExplorer" ); }
		}

		public void Initialize() {
			InitializeLists();
		}

		public void Shutdown() { }

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
								var decadeTagList = mTagManager.DecadeTagList;

								foreach( var album in albumList.List ) {
									var publishedYear = album.PublishedYear;
									var decadeTag = decadeTagList.Where( decade => publishedYear >= decade.StartYear && publishedYear <= decade.EndYear ).FirstOrDefault();

									if( decadeTag != null ) {
										var associationTag = new DbTagAssociation( eTagGroup.Decade, decadeTag.DbId, artistId, album.DbId );

										mTagAssociationProvider.AddAssociation( associationTag );
									}
								}

								NoiseLogger.Current.LogMessage( string.Format( "Built decade tag associations for: {0}", artist.Name ));
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
