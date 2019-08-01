using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
    class UserTagManager : IUserTagManager {
        private readonly IEventAggregator           mEventAggregator; 
        private readonly ITagAssociationProvider    mAssociationProvider;
        private readonly ITagProvider               mTagProvider;
        private readonly IAlbumProvider             mAlbumProvider;

        public UserTagManager( IAlbumProvider albumProvider, ITagProvider tagProvider, ITagAssociationProvider associationProvider, IEventAggregator eventAggregator ) {
            mAlbumProvider = albumProvider;
            mEventAggregator = eventAggregator;
            mTagProvider = tagProvider;
            mAssociationProvider = associationProvider;
        }

        public IEnumerable<DbTag> GetUserTagList() {
            var retValue = new List<DbTag>();

            using( var list = mTagProvider.GetTagList( eTagGroup.User )) {
                retValue.AddRange( list.List );
            }

            return retValue;
        }

        public IEnumerable<DbTag> GetAssociatedTags( long forEntity ) {
            var retValue = new List<DbTag>();

            using( var list = mAssociationProvider.GetArtistTagList( forEntity, eTagGroup.User )){
                foreach( var association in list.List ) {
                    retValue.Add( mTagProvider.GetTag( association.TagId ));
                }
            }

            return retValue;
        }

        public void UpdateAssociations( DbTrack forTrack, IEnumerable<DbTag> tags ) {
            var newTags = tags.ToList();
            var currentTags = GetAssociatedTags( forTrack.DbId ).ToList();
            var updates = false;

            foreach( var tag in newTags ) {
                var current = currentTags.FirstOrDefault( t => t.DbId.Equals( tag.DbId ));

                if( current == null ) {
                    mAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, tag.DbId, forTrack.DbId, Constants.cDatabaseNullOid ));

                    updates = true;
                }
            }

            using( var currentAssociations = mAssociationProvider.GetArtistTagList( forTrack.DbId, eTagGroup.User )) {
                foreach( var tag in currentTags ) {
                    var current = newTags.FirstOrDefault( t => t.DbId.Equals( tag.DbId ));

                    if( current == null ) {
                        var association = currentAssociations.List.FirstOrDefault( a => a.TagId.Equals( tag.DbId ));

                        if( association != null ) {
                            mAssociationProvider.RemoveAssociation( association.DbId );

                            updates = true;
                        }
                    }
                }
            }

            if( updates ) {
                // bump the version so the sidecar gets an update
                using( var album = mAlbumProvider.GetAlbumForUpdate( forTrack.Album )) {
                    album.Item.UpdateVersion();

                    album.Update();
                }

                mEventAggregator.PublishOnUIThread( new Events.UserTagsChanged());
            }
        }

        public IEnumerable<DbTagAssociation> GetAssociations( long forTagId ) {
            var retValue = new List<DbTagAssociation>();

            using( var list = mAssociationProvider.GetTagList( eTagGroup.User, forTagId )) {
                retValue.AddRange( list.List );
            }

            return retValue;
        }

        public void DeleteTag( DbTag tag ) {
            var associations = GetAssociations( tag.DbId );

            foreach( var association in associations ) {
                mAssociationProvider.RemoveAssociation( association.DbId );
            }

            mTagProvider.DeleteTag( tag );
            mEventAggregator.PublishOnUIThread(new Events.UserTagsChanged());
        }

        public void DeleteAssociation( DbTagAssociation association ) {
            mAssociationProvider.RemoveAssociation( association.DbId );

            mEventAggregator.PublishOnUIThread(new Events.UserTagsChanged());
        }
    }
}
