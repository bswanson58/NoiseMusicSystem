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
		private const string		cDecadeTagBuilderId		= "ComponentId_TagBuilder";

		private readonly IDatabaseManager	mDatabaseMgr;
		private readonly IArtistProvider	mArtistProvider;
		private readonly ITimestampProvider	mTimestampProvider;
		private readonly ITagManager		mTagManager;
		private List<long>					mArtistList;
		private IEnumerator<long>			mArtistEnum;
		private long						mLastScanTicks;
		private	long						mStartScanTicks;

		public DecadeTagBuilder( ILifecycleManager lifecycleManager, IDatabaseManager databaseManager, ITimestampProvider timestampProvider, IArtistProvider artistProvider, ITagManager tagManager ) {
			mDatabaseMgr = databaseManager;
			mTimestampProvider = timestampProvider;
			mArtistProvider = artistProvider;
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
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mLastScanTicks = mTimestampProvider.GetTimestamp( cDecadeTagBuilderId );
				mStartScanTicks = DateTime.Now.Ticks;

				mArtistList = new List<long>( from DbArtist artist in database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>() select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
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
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				var artistId = NextArtist();

				if( artistId != 0 ) {
					var artist = mArtistProvider.GetArtist( artistId );

					if(( artist != null ) &&
					   ( artist.LastChangeTicks > mLastScanTicks )) {
						var parms = database.Database.CreateParameters();
						parms["artistId"] = artistId;

						var currentTagList = database.Database.ExecuteQuery( "SELECT DbTagAssociation WHERE ArtistId = @artistId", parms ).OfType<DbTagAssociation>();
						foreach( var tag in currentTagList ) {
							database.Delete( tag );
						}

						var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
						var decadeTagList = mTagManager.DecadeTagList;

						foreach( var album in albumList ) {
							var publishedYear = album.PublishedYear;
							var decadeTag = decadeTagList.Where( decade => publishedYear >= decade.StartYear && publishedYear <= decade.EndYear ).FirstOrDefault();

							if( decadeTag != null ) {
								var associationTag = new DbTagAssociation( eTagGroup.Decade, decadeTag.DbId, artistId, album.DbId );

								database.Insert( associationTag );
							}
						}

						NoiseLogger.Current.LogMessage( string.Format( "Built decade tag associations for: {0}", artist.Name ));
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DecadeTagBuilder:Task ", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}
	}
}
