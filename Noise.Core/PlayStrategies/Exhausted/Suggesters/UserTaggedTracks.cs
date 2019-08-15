using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class UserTaggedTracks : ExhaustedHandlerBase, IDisposable,
                             IHandle<Events.UserTagsChanged> {
        private IEventAggregator                mEventAggregator;
        private readonly ITrackProvider         mTrackProvider;
        private readonly ITagProvider           mTagProvider;
        private readonly IUserTagManager        mTagManager;
        private readonly List<DbTagAssociation> mAssociationList;
        private long                            mTagId;

        public UserTaggedTracks( IUserTagManager tagManager, ITagProvider tagProvider, ITrackProvider trackProvider, IEventAggregator eventAggregator )
            : base( eTrackPlayHandlers.PlayUserTags, eTrackPlayStrategy.Suggester, "Play Tagged Tracks", "Play tracks associated with a tag" ) {
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mTrackProvider = trackProvider;
            mEventAggregator = eventAggregator;

            mAssociationList = new List<DbTagAssociation>();

            RequiresParameters = true;

            mEventAggregator.Subscribe( this );
        }

        public void Dispose() {
            mEventAggregator?.Unsubscribe( this );
            mEventAggregator = null;
        }

        public override void InitialConfiguration( ExhaustedStrategySpecification specification ) {
            var tag = mTagProvider.GetTag( specification.SuggesterParameter );

            if( tag != null ) {
                SetDescription( $"play tracks associated with tag '{tag.Name}'" );
            }
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            if(( context.SuggesterParameter != mTagId ) ||
               (!mAssociationList.Any())) {
                LoadAssociations( context.SuggesterParameter );
            }

            if( mAssociationList.Any()) {
                var association = mAssociationList.Skip( NextRandom( mAssociationList.Count )).Take( 1 ).FirstOrDefault();

                if( association != null ) {
                    AddSuggestedTrack( mTrackProvider.GetTrack( association.ArtistId ), context );
                }
            }
        }

        public void Handle( Events.UserTagsChanged args ) {
            LoadAssociations( mTagId );
        }

        private void LoadAssociations( long tagId ) {
            if( tagId != Constants.cDatabaseNullOid ) {
                mTagId = tagId;

                mAssociationList.Clear();
                mAssociationList.AddRange( mTagManager.GetAssociations( tagId ));
            }
        }
    }
}
