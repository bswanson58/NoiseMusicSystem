using System;
using System.Linq;
using System.Xml.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal class ImportFavorites : IDataImport {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;

		public ImportFavorites( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public int Import( XElement rootElement, bool eliminateDuplicates ) {
			var retValue = 0;
			var favoriteList = from element in rootElement.Descendants( ExchangeConstants.cFavoriteItem ) select element;

			using( var artistList = mArtistProvider.GetArtistList()) {
				foreach( var favorite in favoriteList ) {
					var artist = (string)favorite.Element( ExchangeConstants.cArtist );

					if(!string.IsNullOrWhiteSpace( artist )) {
						var dbArtist = ( from DbArtist a in artistList.List 
										 where a.Name.Equals( artist, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
						if( dbArtist != null ) {
							var album = (string)favorite.Element( ExchangeConstants.cAlbum );

							if(!string.IsNullOrEmpty( album )) {
								using( var albumList = mAlbumProvider.GetAlbumList( dbArtist )) {
									var dbAlbum = ( from DbAlbum a in albumList.List
													where a.Name.Equals( album, StringComparison.CurrentCultureIgnoreCase ) select a ).FirstOrDefault();
									if( dbAlbum != null ) {
										var track = (string)favorite.Element( ExchangeConstants.cTrack );

										if(!string.IsNullOrWhiteSpace( track )) {
											using( var trackList = mTrackProvider.GetTrackList( dbAlbum )) {
												var dbTrack = ( from DbTrack t in trackList.List
																where t.Name.Equals( track, StringComparison.CurrentCultureIgnoreCase ) select t ).FirstOrDefault();
												if( dbTrack != null ) {
													if(!dbTrack.IsFavorite ) {
														GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbTrack.DbId, true ));

														retValue++;
													}
												}
											}
										}
										else {
											if(!dbAlbum.IsFavorite ) {
												GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbAlbum.DbId, true ));

												retValue++;
											}
										}
									}
								}
							}
							else {
								if(!dbArtist.IsFavorite ) {
									GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( dbArtist.DbId, true ));

									retValue++;
								}
							}
						}
					}
				}
			}

			return( retValue );
		}
	}
}
