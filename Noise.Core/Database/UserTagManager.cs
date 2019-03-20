using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
    class UserTagManager : IUserTagManager {
        private readonly    ITagAssociationProvider mAssociationProvider;
        private readonly    ITagProvider            mTagProvider;

        public UserTagManager( ITagProvider tagProvider, ITagAssociationProvider associationProvider ) {
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

        public void UpdateAssociations( long forEntity, IEnumerable<DbTag> tags ) {
            var newTags = tags.ToList();
            var currentTags = GetAssociatedTags( forEntity ).ToList();

            foreach( var tag in newTags ) {
                var current = currentTags.FirstOrDefault( t => t.DbId.Equals( tag.DbId ));

                if( current == null ) {
                    mAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, tag.DbId, forEntity, 0L ));
                }
            }

            using( var currentAssociations = mAssociationProvider.GetArtistTagList( forEntity, eTagGroup.User )) {
                foreach( var tag in currentTags ) {
                    var current = newTags.FirstOrDefault( t => t.DbId.Equals( tag.DbId ));

                    if( current == null ) {
                        var association = currentAssociations.List.FirstOrDefault( a => a.TagId.Equals( tag.DbId ));

                        if( association != null ) {
                            mAssociationProvider.RemoveAssociation( association.DbId );
                        }
                    }
                }
            }
        }

        public IEnumerable<DbTagAssociation> GetAssociations( long forTagId ) {
            var retValue = new List<DbTagAssociation>();

            using( var list = mAssociationProvider.GetTagList( eTagGroup.User, forTagId )) {
                retValue.AddRange( list.List );
            }

            return retValue;
        }
    }
}
