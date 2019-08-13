using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    abstract class ExhaustedHandlerBase : IExhaustedPlayHandler {
        private readonly Random     mRandom;

        public  eTrackPlayHandlers  Identifier { get; }
        public eTrackPlayStrategy   StrategyType { get; }
        public  string              Name { get; }
        public  string              Description { get; }
        public  bool                RequiresParameters { get; protected set; }

        protected ExhaustedHandlerBase( eTrackPlayHandlers handlerId, eTrackPlayStrategy type, string name, string description ) {
            Identifier = handlerId;
            StrategyType = type;
            Name = name;
            Description = description;

            RequiresParameters = false;
            mRandom = new Random( DateTime.Now.Millisecond );
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
