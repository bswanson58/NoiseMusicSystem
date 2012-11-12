using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class DiscographyExplorer : IBackgroundTask,
									   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IMetadataManager		mMetadataManager;
		private List<long>						mArtistList;
		private IEnumerator<long>				mArtistEnum;

		public DiscographyExplorer( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider,
									IMetadataManager metadataManager ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mMetadataManager = metadataManager;
	
			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return( "Task_DiscographyExplorer" ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
		}

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
					var artist = mArtistProvider.GetArtist( artistId );

					if( artist != null ) {
						var discography = mMetadataManager.GetArtistDiscography( artist.Name );
						var uniqueList = ReduceList( discography.Discography );

						DatabaseCache<DbAlbum>	albumCache;
						using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
							albumCache = new DatabaseCache<DbAlbum>( from DbAlbum album in albumList.List
																	 where album.PublishedYear == Constants.cUnknownYear select album );
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

								NoiseLogger.Current.LogMessage( string.Format( "Updating Published year from discography: album '{0}', year: '{1}'",
																				dbAlbum.Name, release.Year ));
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DiscographyExplorer:Task ", ex );
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
