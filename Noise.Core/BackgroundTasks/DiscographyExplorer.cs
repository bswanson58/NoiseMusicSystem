using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Noise.Core.Database;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class DiscographyExplorer : IBackgroundTask, IRequireInitialization {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IDiscographyProvider	mDiscographyProvider;
		private List<long>						mArtistList;
		private IEnumerator<long>				mArtistEnum;

		public DiscographyExplorer( ILifecycleManager lifecycleManager, IArtistProvider artistProvider, IAlbumProvider albumProvider, IDiscographyProvider discographyProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mDiscographyProvider = discographyProvider;
	
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
				InitializeLists();

				mArtistEnum.MoveNext();
			}

			return( mArtistEnum.Current );
		}

		public void ExecuteTask() {
			try {
				var artistId = NextArtist();

				if( artistId != 0 ) {
					var	discography = new List<DbDiscographyRelease>();
					using( var discographyList = mDiscographyProvider.GetDiscography( artistId )) {
						discography.AddRange( discographyList.List );
					}
					var uniqueList = ReduceList( discography );

					DatabaseCache<DbAlbum>	albumCache;
					using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
						albumCache = new DatabaseCache<DbAlbum>( from DbAlbum album in albumList.List where album.PublishedYear == Constants.cUnknownYear select album );
					}

					foreach( var release in uniqueList ) {
						var releaseTitle = release.Title;
						var	dbAlbum = albumCache.Find( album => album.Name.Equals( releaseTitle, StringComparison.CurrentCultureIgnoreCase ));

						if( dbAlbum != null ) {
							using( var updater = mAlbumProvider.GetAlbumForUpdate( dbAlbum.DbId )) {
								if( updater.Item != null ) {
									updater.Item.PublishedYear = release.Year;

									updater.Update();
								}
							}

							NoiseLogger.Current.LogMessage( string.Format( "Updating Published year from discography: album '{0}', year: '{1}'", dbAlbum.Name, dbAlbum.PublishedYear ));
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DiscographyExplorer:Task ", ex );
			}
		}

		private static IEnumerable<DbDiscographyRelease> ReduceList( IEnumerable<DbDiscographyRelease> list ) {
			var uniqueList = new Dictionary<string, DbDiscographyRelease>();

			foreach( var release in list ) {
				if(( release.ReleaseType == DiscographyReleaseType.Release ) &&
				   ( release.Year != 0 )) {
					if(!uniqueList.ContainsKey( release.Title )) {
						uniqueList.Add( release.Title, release );
					}
					else {
						var currentRelease = uniqueList[release.Title];

						if( release.Year < currentRelease.Year ) {
							uniqueList[release.Title] = currentRelease;
						}
					}
				}
			}

			return( uniqueList.Values.ToList());
		}
	}
}
