using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    abstract class ExhaustedHandlerBase : IExhaustedPlayHandler {
        private readonly Random     mRandom;

        public  string              ItemIdentity { get; }
        public  string              Name { get; }
        public  string              Description { get; }

        private ExhaustedHandlerBase( string identity, string name, string description ) {
            ItemIdentity = identity;
            Name = name;
            Description = description;

            mRandom = new Random( DateTime.Now.Millisecond );
        }

        protected ExhaustedHandlerBase( eTrackPlaySuggesters handlerId, string name, string description ) :
            this( handlerId.ToString(), name, description ) {
        }

        protected ExhaustedHandlerBase( eTrackPlayDisqualifiers handlerId, string name, string description ) :
            this( handlerId.ToString(), name, description ) {
        }

        public abstract void SelectTrack( IExhaustedSelectionContext context );

        protected DbTrack SelectRandomTrack( IEnumerable<DbTrack> fromList ) {
            return( SelectRandomTrack( fromList.ToList()));
        }

        protected DbTrack SelectRandomTrack( IList<DbTrack> fromList ) {
            var next = mRandom.Next( fromList.Count - 1 );

            return fromList.Skip( next ).FirstOrDefault();
        }
    }
}
