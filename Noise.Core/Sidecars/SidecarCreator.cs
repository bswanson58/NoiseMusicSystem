using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarCreator : ISidecarCreator {
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
        private readonly IUserTagManager                mTagManager;
        private readonly ITagProvider                   mTagProvider;
        private readonly ITagAssociationProvider        mAssociationProvider;

		public SidecarCreator( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, 
                               IUserTagManager tagManager, ITagProvider tagProvider, ITagAssociationProvider associationProvider, ILogLibraryBuildingSidecars log ) {
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mAssociationProvider = associationProvider;
		}

		public ScArtist CreateFrom( DbArtist artist ) {
			return( new ScArtist( artist ));
		}

		public ScAlbum CreateFrom( DbAlbum album ) {
			var retValue = new ScAlbum( album );

			if( album != null ) {
				using( var trackList = mTrackProvider.GetTrackList( album )) {
					foreach( var track in trackList.List ) {
                        var sidecar = new ScTrack( track );

                        AddTags( sidecar, track );
						retValue.TrackList.Add( sidecar );
					}
				}
			}

			return( retValue );
		}

		public ScAlbum CreateFrom( DbTrack track ) {
			return( CreateFrom( mAlbumProvider.GetAlbum( track.Album )));
		}

        private void AddTags( ScTrack sidecar, DbTrack forTrack ) {
            sidecar.Tags.AddRange( from t in mTagManager.GetAssociatedTags( forTrack.DbId ) select t.Name );
        }

		public void Update( DbArtist artist, ScArtist sidecar ) {
			using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
				if( updater.Item != null ) {
					sidecar.UpdateArtist( updater.Item );
					updater.Item.SetVersionPreUpdate( sidecar.Version );

					updater.Update();

					sidecar.UpdateArtist( artist );
					mLog.LogUpdated( artist, sidecar );
				}
			}
		}

		public void Update( DbAlbum album, ScAlbum sidecar ) {
			using( var updater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
				if( updater.Item != null ) {
					sidecar.UpdateAlbum( updater.Item );
					updater.Item.SetVersionPreUpdate( sidecar.Version );

					updater.Update();

					sidecar.UpdateAlbum( album );
					mLog.LogUpdated( album, sidecar );
				}
			}

			using( var trackList = mTrackProvider.GetTrackList( album )) {
				foreach( var scTrack in sidecar.TrackList ) {
					var dbTrack = trackList.List.FirstOrDefault( track => track.Name.Equals( scTrack.TrackName, StringComparison.CurrentCultureIgnoreCase ) &&
																		  track.TrackNumber == scTrack.TrackNumber &&
																		  track.VolumeName.Equals( scTrack.VolumeName, StringComparison.CurrentCultureIgnoreCase ));
					if( dbTrack != null ) {
						using( var updater = mTrackProvider.GetTrackForUpdate( dbTrack.DbId )) {
							if( updater.Item != null ) {
								scTrack.UpdateTrack( updater.Item );

								updater.Update();
							}
						}

                        UpdateTags( scTrack.Tags, dbTrack );
					}
					else {
						mLog.LogUnknownTrack( album, scTrack );
					}
				}
			}
		}

        private void UpdateTags( IList<string> tags, DbTrack forTrack ) {
            var tagList = mTagManager.GetUserTagList().ToList();
            var currentTags = mTagManager.GetAssociatedTags( forTrack.DbId ).ToList();

            foreach( var tag in tags ) {
                var existingTag = currentTags.FirstOrDefault( t => t.Name.Equals( tag ));

                if( existingTag == null ) {
                    var userTag = tagList.FirstOrDefault( t => t.Name.Equals( tag ));

                    if( userTag == null ) {
                        userTag = new DbTag( eTagGroup.User, tag );

                        mTagProvider.AddTag( userTag );
                    }

                    mAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, userTag.DbId, forTrack.DbId, Constants.cDatabaseNullOid ));
                }
            }
        }
	}
}
