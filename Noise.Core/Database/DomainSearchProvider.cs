using System;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DomainSearchProvider : IDomainSearchProvider {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IDbBaseProvider	mDbBaseProvider;
		private readonly IDatabaseInfo		mDatabaseInfo;

		public DomainSearchProvider( IDatabaseInfo databaseInfo,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvide, IDbBaseProvider dbBaseProvider ) {
			mDatabaseInfo = databaseInfo;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvide;
			mDbBaseProvider = dbBaseProvider;
		}

		public DataFindResults Find( string artist, string album, string track ) {
			DataFindResults	retValue = null;

			try {
				var databaseId = mDatabaseInfo.DatabaseId;

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
												retValue = dbTrack != null ? new DataFindResults( databaseId, dbArtist, dbAlbum, dbTrack, true ) :
																			 new DataFindResults( databaseId, dbArtist, dbAlbum, false );
											}
										}
										else {
											retValue = new DataFindResults( databaseId, dbArtist, dbAlbum, true );
										}
									}
									else {
										retValue = new DataFindResults( databaseId, dbArtist, false );
									}
								}
							}
							else {
								retValue = new DataFindResults( databaseId, dbArtist, true );
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
			var				databaseId = mDatabaseInfo.DatabaseId;
			var				item = mDbBaseProvider.GetItem( itemId );

			if( item != null ) {
				TypeSwitch.Do( item, TypeSwitch.Case<DbArtist>( artist => retValue = new DataFindResults( databaseId, artist, true )),
									 TypeSwitch.Case<DbAlbum>( album => {
									                           		var artist = mArtistProvider.GetArtistForAlbum( album );
																	if( artist != null ) {
																		retValue = new DataFindResults( databaseId, mArtistProvider.GetArtistForAlbum( album ), album, true );
																	}
																}),
									 TypeSwitch.Case<DbTrack>( track => {
									                           		var album = mAlbumProvider.GetAlbumForTrack( track );
																	if( album != null ) {
																		var artist = mArtistProvider.GetArtistForAlbum( album );
																		if( artist != null ) {
																			retValue = new DataFindResults( databaseId, artist, album, track, true );
																		}
																	}
									                           }));
			}

			return( retValue );
		}
	}
}
