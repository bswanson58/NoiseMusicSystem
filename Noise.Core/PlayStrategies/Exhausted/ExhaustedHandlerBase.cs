using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    abstract class ExhaustedHandlerBase : IExhaustedPlayHandler {
        private readonly Random     mRandom;

        public  string              HandlerEnum { get; }

        protected ExhaustedHandlerBase( eTrackPlaySuggesters handlerId ) {
            HandlerEnum = handlerId.ToString();

            mRandom = new Random( DateTime.Now.Millisecond );
        }

        protected ExhaustedHandlerBase( eTrackPlayDisqualifiers handlerId ) {
            HandlerEnum = handlerId.ToString();
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
