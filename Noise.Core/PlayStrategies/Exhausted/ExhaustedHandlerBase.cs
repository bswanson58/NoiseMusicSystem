using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    abstract class ExhaustedHandlerBase : IExhaustedPlayHandler {
        private readonly Random     mRandom;

        public  eTrackPlayHandlers  Identifier { get; }
        public  eTrackPlayStrategy  StrategyType { get; }
        public  string              Name { get; }
        public  string              Description { get; private set; }
        public  bool                RequiresParameters { get; protected set; }

        protected ExhaustedHandlerBase( eTrackPlayHandlers handlerId, eTrackPlayStrategy type, string name, string description ) {
            Identifier = handlerId;
            StrategyType = type;
            Name = name;
            Description = description;

            RequiresParameters = false;
            mRandom = new Random( DateTime.Now.Millisecond );
        }

        public virtual void InitialConfiguration( ExhaustedStrategySpecification specification ) { }
        public abstract void SelectTrack( IExhaustedSelectionContext context );

        protected void SetDescription( string description ) {
            Description = description;
        }

        protected int NextRandom( int maxValue ) {
            var	retValue = 0;

            if( maxValue > 0 ) {
                retValue = mRandom.Next( maxValue );
            }

            return( retValue );
        }

        protected DbTrack SelectRandomTrack( IEnumerable<DbTrack> fromList ) {
            return( SelectRandomTrack( fromList.ToList()));
        }

        protected DbTrack SelectRandomTrack( IList<DbTrack> fromList ) {
            return fromList.Skip( NextRandom( fromList.Count - 1 )).FirstOrDefault();
        }

        protected bool AddSuggestedTrack( DbTrack track, IExhaustedSelectionContext context ) {
            var retValue = false;

            if(( track != null ) &&
               ( context.SelectedTracks.All( t => t.DbId != track.DbId ))) {
                context.SelectedTracks.Add( track );

                retValue = true;
            }

            return retValue;
        }
    }
}
