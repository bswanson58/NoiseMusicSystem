using System;
using System.Linq;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DomainSearchProvider : IDomainSearchProvider, IRequireInitialization {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IDbBaseProvider	mDbBaseProvider;
		private readonly IDatabaseManager	mDatabaseManager;
		private long						mDatabaseId;

		public DomainSearchProvider( ILifecycleManager lifecycleManager, IDatabaseManager databaseManager,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvide, IDbBaseProvider dbBaseProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvide;
			mDbBaseProvider = dbBaseProvider;
			mDatabaseManager = databaseManager;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			IDatabase	database = null;

			try {
				database = mDatabaseManager.ReserveDatabase();
				mDatabaseId = database.DatabaseVersion.DatabaseId;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DomainSearchProvider:DatabaseId", ex );
			}
			finally {
				if( database != null ) {
					mDatabaseManager.FreeDatabase( database );
				}
			}
		}

		public void Shutdown() { }

		public DataFindResults Find( string artist, string album, string track ) {
			DataFindResults	retValue = null;

			try {
				if(!string.IsNullOrWhiteSpace( artist )) {
					using( var artistList = mArtistProvider.GetArtistList()) {
						var dbArtist = ( from DbArtist a in artistList.List 
											where a.Name.Equals( artist, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
						if( dbArtist != null ) {
							if(!string.IsNullOrEmpty( album )) {
								using( var albumList = mAlbumProvider.GetAlbumList( dbArtist )) {
									var dbAlbum = ( from DbAlbum a in albumList.List
													where a.Name.Equals( album, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
									if( dbAlbum != null ) {
										if(!string.IsNullOrWhiteSpace( track )) {
											using( var trackList = mTrackProvider.GetTrackList( dbAlbum )) {
												var dbTrack = ( from DbTrack t in trackList.List
																where t.Name.Equals( track, StringComparison.CurrentCultureIgnoreCase ) select t ).FirstOrDefault();
												retValue = dbTrack != null ? new DataFindResults( mDatabaseId, dbArtist, dbAlbum, dbTrack, true ) :
																			 new DataFindResults( mDatabaseId, dbArtist, dbAlbum, false );
											}
										}
										else {
											retValue = new DataFindResults( mDatabaseId, dbArtist, dbAlbum, true );
										}
									}
									else {
										retValue = new DataFindResults( mDatabaseId, dbArtist, false );
									}
								}
							}
							else {
								retValue = new DataFindResults( mDatabaseId, dbArtist, true );
							}
						}
					}
				}		
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DataProvider:Find: ", ex );
			}

			return( retValue );
		}

		public DataFindResults Find( long itemId ) {
			DataFindResults	retValue = null;
			var item = mDbBaseProvider.GetItem( itemId );

			if( item != null ) {
				TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => retValue = new DataFindResults( mDatabaseId, artist, true )),
									 TypeSwitch.Case<DbAlbum>( album => {
									                           		var artist = mArtistProvider.GetArtistForAlbum( album );
																	if( artist != null ) {
																		retValue = new DataFindResults( mDatabaseId, mArtistProvider.GetArtistForAlbum( album ), album, true );
																	}
																}),
									 TypeSwitch.Case<DbTrack>( track => {
									                           		var album = mAlbumProvider.GetAlbumForTrack( track );
																	if( album != null ) {
																		var artist = mArtistProvider.GetArtistForAlbum( album );
																		if( artist != null ) {
																			retValue = new DataFindResults( mDatabaseId, artist, album, track, true );
																		}
																	}
									                           }));
			}

			return( retValue );
		}
	}
}
