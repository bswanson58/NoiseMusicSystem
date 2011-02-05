using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	internal class DecadeTagBuilder : IBackgroundTask {
		private const string		cDecadeTagBuilderId		= "ComponentId_TagBuilder";

		private IUnityContainer		mContainer;
		private IDatabaseManager	mDatabaseMgr;
		private	INoiseManager		mNoiseManager;
		private ILog				mLog;
		private List<long>			mArtistList;
		private IEnumerator<long>	mArtistEnum;
		private long				mLastScanTicks;
		private	long				mStartScanTicks;
		public string TaskId {
			get { return( "Task_DiscographyExplorer" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;
			mDatabaseMgr = mContainer.Resolve<IDatabaseManager>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			InitializeLists();

			return( true );
		}

		private void InitializeLists() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mLastScanTicks = mNoiseManager.DataProvider.GetTimestamp( cDecadeTagBuilderId );
				mStartScanTicks = DateTime.Now.Ticks;

				mArtistList = new List<long>( from DbArtist artist in database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>() select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		private long NextArtist() {
			if(!mArtistEnum.MoveNext()) {
				mNoiseManager.DataProvider.SetTimestamp( cDecadeTagBuilderId, mStartScanTicks );

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
					var artist = mNoiseManager.DataProvider.GetArtist( artistId );

					if(( artist != null ) &&
					   ( artist.LastChangeTicks > mLastScanTicks )) {
						var parms = database.Database.CreateParameters();
						parms["artistId"] = artistId;

						var currentTagList = database.Database.ExecuteQuery( "SELECT DbTagAssociation WHERE ArtistId = @artistId", parms ).OfType<DbTagAssociation>();
						foreach( var tag in currentTagList ) {
							database.Delete( tag );
						}

						var albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
						var decadeTagList = mNoiseManager.TagManager.DecadeTagList;

						foreach( var album in albumList ) {
							var publishedYear = album.PublishedYear;
							var decadeTag = decadeTagList.Where( decade => publishedYear >= decade.StartYear && publishedYear <= decade.EndYear ).FirstOrDefault();

							if( decadeTag != null ) {
								var associationTag = new DbTagAssociation( eTagGroup.Decade, decadeTag.DbId, artistId, album.DbId );

								database.Insert( associationTag );
							}
						}

						mLog.LogMessage( string.Format( "Built decade tag associations for: {0}", artist.Name ));
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - DecadeTagBuilder:Task ", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		public void Shutdown() {
		}
	}
}
