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
	public class LinkSimilarArtists : IBackgroundTask, IRequireInitialization {
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;
		private DatabaseCache<DbArtist>					mArtistCache;
		private List<long>								mSimilarArtistLists;
		private IEnumerator<long>						mListEnum;

		public LinkSimilarArtists( ILifecycleManager lifecycleManager, IArtistProvider artistProvider, IAssociatedItemListProvider associatedItemListProvider ) {
			mArtistProvider = artistProvider;
			mAssociationProvider = associatedItemListProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_LinkSimilarArtists" ); }
		}

		public void Initialize() {
			InitializeLists();
		}

		public void Shutdown() { }

		private void InitializeLists() {
			try {
				using( var artistList = mArtistProvider.GetArtistList()) {
					mArtistCache = new DatabaseCache<DbArtist>( from DbArtist artist in artistList.List select artist );
				}

				using( var associationList = mAssociationProvider.GetAssociatedItemLists( ContentType.SimilarArtists )) {
					mSimilarArtistLists = new List<long>( from DbAssociatedItemList list in associationList.List select list.DbId );
				}
				mListEnum = mSimilarArtistLists.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LinkSimilarArtists:InitializeLists ", ex );
			}
		}

		public void ExecuteTask() {
			var listId = NextList();

			if( listId != 0 ) {
				try {
					using( var updater = mAssociationProvider.GetAssociationForUpdate( listId )) {
						if( updater.Item != null ) {
							var	needUpdate = false;

							foreach( var similarArtist in updater.Item.Items ) {
								var artistName = similarArtist.Item;
								var dbArtist = mArtistCache.Find( artist => String.Compare( artist.Name, artistName, true ) == 0 );

								if( dbArtist != null ) {
									if( similarArtist.AssociatedId != dbArtist.DbId ) {
										similarArtist.SetAssociatedId( dbArtist.DbId );

										needUpdate = true;
									}
								}
								else {
									if( similarArtist.IsLinked ) {
										similarArtist.SetAssociatedId( Constants.cDatabaseNullOid );

										needUpdate = true;
									}
								}
							}

							if( needUpdate ) {
								updater.Update();

								NoiseLogger.Current.LogMessage( "Updated links to similar artists." );
							}
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LinkSimilarArtist Task:", ex );
				}
			}
		}

		private long NextList() {
			if(!mListEnum.MoveNext()) {
				InitializeLists();

				mListEnum.MoveNext();
			}

			return( mListEnum.Current );
		}
	}
}
