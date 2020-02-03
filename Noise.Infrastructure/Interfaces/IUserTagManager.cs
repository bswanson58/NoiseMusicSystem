using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IUserTagManager {
        IEnumerable<DbTag>      GetUserTagList();
        IEnumerable<DbTag>      GetAssociatedTags( long forEntity );
        void                    UpdateAssociations( DbTrack forTrack, IEnumerable<DbTag> tags );

        IEnumerable<DbTagAssociation>   GetAssociations( long forTagId );

        void                    DeleteTag( DbTag tag );
        void                    DeleteAssociation( DbTagAssociation association );
    }
}
