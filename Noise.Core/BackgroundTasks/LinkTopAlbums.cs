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
	internal class LinkTopAlbums : IBackgroundTask, IRequireInitialization {
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;
		private List<long>								mArtistList;
		private IEnumerator<long>						mArtistEnum;

		public LinkTopAlbums( ILifecycleManager lifecycleManager, IArtistProvider artistProvider, IAlbumProvider albumProvider, IAssociatedItemListProvider associatedItemListProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mAssociationProvider = associatedItemListProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_LinkTopAlbums" ); }
		}

		public void Shutdown() { }

		public void Initialize() {
			InitializeLists();
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
			var artistId = NextArtist();

			if( artistId != 0 ) {
				try {
					var topAlbums = mAssociationProvider.GetAssociatedItems( artistId, ContentType.TopAlbums );
					if( topAlbums != null ) {
						using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
							var needUpdate = false;

							using( var updater = mAssociationProvider.GetAssociationForUpdate( topAlbums.DbId )) {
								foreach( var topAlbum in topAlbums.Items ) {
									var topAlbumName = topAlbum.Item;
									var	dbAlbum = albumList.List.FirstOrDefault( album => album.Name.Equals( topAlbumName, StringComparison.CurrentCultureIgnoreCase ));

									if( dbAlbum != null ) {
										if(( topAlbum.AssociatedId != dbAlbum.DbId ) ||
										   ( topAlbum.IsLinked == false )) {
											topAlbum.SetAssociatedId( dbAlbum.DbId );

											needUpdate = true;
										}
									}

									if(( dbAlbum == null ) &&
									   ( topAlbum.IsLinked )) {
										topAlbum.SetAssociatedId( Constants.cDatabaseNullOid );

										needUpdate = true;
									}
								}

								if( needUpdate ) {
									updater.Update();

									NoiseLogger.Current.LogMessage( "Updated links to top albums" );
								}
							}
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LinkTopAlbums:Task ", ex );
				}
			}
		}
	}
}
