using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
    class PlayExhaustedStrategyUserTags : PlayExhaustedStrategyRandomBase, IHandle<Events.UserTagsChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly ITrackProvider         mTrackProvider;
        private readonly ITagProvider           mTagProvider;
        private readonly IUserTagManager        mTagManager;
        private readonly List<DbTagAssociation> mAssociationList;
        private long                            mTagId;
        private string                          mTagName;

        public PlayExhaustedStrategyUserTags( IUserTagManager tagManager, ITagProvider tagProvider, ITrackProvider trackProvider, ILogPlayStrategy log, IEventAggregator eventAggregator ) :
            base( ePlayExhaustedStrategy.PlayUserTags, "Play Tagged Tracks", "Play tracks associated with a tag", "Tag", null, null, log ) {
            mEventAggregator = eventAggregator;
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mTrackProvider = trackProvider;

            mAssociationList = new List<DbTagAssociation>();
            mEventAggregator.Subscribe( this );
        }

        protected override string FormatDescription() {
            return $"play tracks from tag '{mTagName}'";
        }

        protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
            if( parameters is PlayStrategyParameterDbId parms ) {
                var tag = mTagProvider.GetTag( parms.DbItemId );

                if( tag != null ) {
                    mTagId = tag.DbId;
                    mTagName = tag.Name;

                    LoadAssociations();
                }
            }
        }

        public void Handle( Events.UserTagsChanged args ) {
            LoadAssociations();
        }

        private void LoadAssociations() {
            mAssociationList.Clear();
            mAssociationList.AddRange( mTagManager.GetAssociations( mTagId ));
        }

        protected override DbTrack SelectATrack() {
            var retValue = default( DbTrack );

            if( mAssociationList.Any()) {
                var association = mAssociationList.Skip( NextRandom( mAssociationList.Count )).Take( 1 ).FirstOrDefault();

                if( association != null ) {
                    retValue = mTrackProvider.GetTrack( association.ArtistId );
                }
            }

            return( retValue );
        }
    }
}
