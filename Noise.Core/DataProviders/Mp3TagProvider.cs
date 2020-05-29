using System;
using System.Globalization;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;

namespace Noise.Core.DataProviders {
	public class Mp3TagProvider : IMetadataInfoProvider {
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly ITagManager			mGenreManager;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly INoiseLog				mLog;
		private readonly StorageFile			mFile;
		private	readonly Lazy<File>				mTags;

		public Mp3TagProvider( IArtworkProvider artworkProvider, ITagManager tagManager, IStorageFolderSupport storageFolderSupport, StorageFile file, INoiseLog log ) {
			mArtworkProvider = artworkProvider;
			mGenreManager = tagManager;
			mStorageFolderSupport = storageFolderSupport;
			mLog = log;
			mFile = file;

			Condition.Requires( mFile ).IsNotNull();

			mTags =new Lazy<File>(() => {
				File	retValue = null;

				try {
					retValue = OpenTagFile( mStorageFolderSupport.GetPath( mFile ));
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Getting tags for {0}", mFile ), ex );
				}

				return( retValue );	});
		}

		private File OpenTagFile( string path ) {
			File retValue = null;

			try {
				retValue = File.Create( path );
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Opening file \"{0}\"", path ), ex );
			}

			return( retValue );			
		}

		private File Tags {
			get { return( mTags.Value ); }
		}

		public string Artist {
			get {
				var retValue = "";

				if( Tags != null ) { 
					if(!String.IsNullOrEmpty( Tags.Tag.FirstAlbumArtist )) {
						retValue = Tags.Tag.FirstAlbumArtist;
					}
					else {
						if(!String.IsNullOrEmpty( Tags.Tag.FirstPerformer )) {
							retValue = Tags.Tag.FirstPerformer;
						}
					}
				}

				return( retValue );
			}
		}

		public string Album {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Album;
				}

				return( retValue );
			}
		}

		public string TrackName {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Title;
				}

				return( retValue );
			}
		}

		public string VolumeName {
			get {
				var retValue = "";

				if( Tags != null ) {
					retValue = Tags.Tag.Disc > 0 ? Tags.Tag.Disc.ToString( CultureInfo.InvariantCulture ) : "";
				}

				return( retValue );
			}
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( Tags != null ) {
				try {
					if( Tags.Tag.Year != 0 ) {
						track.PublishedYear = (int)Tags.Tag.Year;
					}

					track.Performer = !String.IsNullOrWhiteSpace( Tags.Tag.FirstPerformer ) ? Tags.Tag.FirstPerformer : artist.Name;

					track.Bitrate = Tags.Properties.AudioBitrate;
					track.SampleRate = Tags.Properties.AudioSampleRate;
					track.DurationMilliseconds = (Int32)Tags.Properties.Duration.TotalMilliseconds;
					track.Channels = (Int16)Tags.Properties.AudioChannels;

					var id3Tags = Tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						track.BeatsPerMinute = (Int16)id3Tags.BeatsPerMinute;

						var frames = id3Tags.GetFrames<UserTextInformationFrame>();
						if( frames != null ) {
							foreach( var fr in frames ) {
								if( fr.Description.IndexOf( "replaygain", StringComparison.InvariantCultureIgnoreCase ) != -1 ) {
									var		desc = fr.Description.ToUpper();
									float	gain;

									if( desc.Contains( "REPLAYGAIN_ALBUM_GAIN" )) {
										if( float.TryParse( FormatReplayGainString( fr.Text[0]), out gain )) {
											track.ReplayGainAlbumGain = gain;
										}
									}
									else if( desc.Contains( "REPLAYGAIN_ALBUM_PEAK" )) {
										if( float.TryParse( fr.Text[0], out gain )) {
											track.ReplayGainAlbumPeak = gain;
										}
									}
									else if( desc.Contains( "REPLAYGAIN_TRACK_GAIN" )) {
										if( float.TryParse( FormatReplayGainString( fr.Text[0]), out gain )) {
											track.ReplayGainTrackGain = gain;
										}
									}
									else if( desc.Contains( "REPLAYGAIN_TRACK_PEAK" )) {
										if( float.TryParse( fr.Text[0], out gain )) {
											track.ReplayGainTrackPeak = gain;
										}
									}
								}
							}
						}

						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, false );
						if( popFrame != null ) {
							track.Rating = mStorageFolderSupport.ConvertFromId3Rating( popFrame.Rating );
							track.PlayCount = (int)popFrame.PlayCount;
						}
						else {
							var popFrames = id3Tags.GetFrames<PopularimeterFrame>();
							if( popFrames != null ) {
								int		playCount = 0;
								var		ratings = 0;
								var		ratingsCount = 0;

								foreach( var fr in popFrames ) {
									playCount += (int)fr.PlayCount;
									if( fr.Rating > 0 ) {
										ratings += mStorageFolderSupport.ConvertFromId3Rating( fr.Rating );
										ratingsCount++;
									}
								}

								track.PlayCount = playCount;
								if( ratingsCount > 0 ) {
									track.Rating = (short)( ratings / ratingsCount );
								}
								else {
									var	ratingFrame = UserTextInformationFrame.Get( id3Tags, "RATING", false );
									if( ratingFrame != null ) {
										var	rating = Convert.ToUInt16( ratingFrame.Text[0]);

										if(( rating > 0 ) &&
										   ( rating <= 5 )) {
											track.Rating = (short)rating;
										}
									}
								}
							}
						}

						var favoritesFrame = UserTextInformationFrame.Get( id3Tags, Constants.FavoriteFrameDescription, false );
						if( favoritesFrame != null ) {
                            if( Boolean.TryParse( favoritesFrame.Text[0], out var isFavorite )) {
								track.IsFavorite = isFavorite;
							}
						}
					}

					var pictures = Tags.Tag.Pictures;
					if(( pictures != null ) &&
					   ( pictures.GetLength( 0 ) > 0 )) {
                        var index = 0;

						foreach( var picture in pictures ) {
							var dbPicture = new DbArtwork( mFile.DbId, picture.Type == PictureType.FrontCover ? ContentType.AlbumCover : ContentType.AlbumArtwork )
									{ Source = InfoSource.Tag,
									  Name = $"Embedded Tag ({index})",
									  Artist = artist.DbId,
									  Album = album.DbId,
									  FolderLocation = mFile.ParentFolder };

							mArtworkProvider.AddArtwork( dbPicture, picture.Data.ToArray());
                            index++;
						}
					}

					if(( track.ExternalGenre == Constants.cDatabaseNullOid ) &&
					   ( Tags.Tag.Genres != null ) &&
					   ( Tags.Tag.Genres.GetLength( 0 ) > 0 )) {
						track.ExternalGenre = mGenreManager.ResolveGenre( Tags.Tag.Genres[0]);
					}
				}
				catch( Exception ex ) {
					mLog.LogException( $"Building tags for {artist}/{album}/{track}", ex );
				}
			}
		}

		private static string FormatReplayGainString( string input ) {
			var retValue = input;

			var index = input.IndexOf( "db", StringComparison.InvariantCultureIgnoreCase );
			if( index != -1 ) {
				retValue = input.Remove( index );
			}

			return( retValue.Trim());
		}
	}
}
