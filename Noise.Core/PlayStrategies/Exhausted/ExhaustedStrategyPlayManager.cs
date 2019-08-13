using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedStrategyPlayManager : IExhaustedStrategyPlayManager {
        private readonly IExhaustedContextFactory       mContextFactory;
        private readonly IExhaustedStrategyFactory      mStrategyFactory;
        private readonly List<IExhaustedPlayHandler>    mPlaySuggesters;
        private readonly List<IExhaustedPlayHandler>    mPlayDisqualifiers;
        private readonly List<IExhaustedPlayHandler>    mPlayBonusHandlers;
        private ExhaustedStrategySpecification          mStrategySpecification;

        public  IStrategyDescription                    CurrentStrategy => mPlaySuggesters.FirstOrDefault();

        public ExhaustedStrategyPlayManager( IExhaustedContextFactory contextFactory, IExhaustedStrategyFactory strategyFactory ) {
            mContextFactory = contextFactory;
            mStrategyFactory = strategyFactory;

            mPlaySuggesters = new List<IExhaustedPlayHandler>();
            mPlayDisqualifiers = new List<IExhaustedPlayHandler>();
            mPlayBonusHandlers = new List<IExhaustedPlayHandler>();
        }

        public ExhaustedStrategySpecification StrategySpecification {
            get => mStrategySpecification;
            set {
                mStrategySpecification = value;

                mStrategyFactory.BuildStrategy( mStrategySpecification, mPlaySuggesters, mPlayDisqualifiers, mPlayBonusHandlers );
            }
        }

        private bool StrategyHasBeenSet() {
            return mPlaySuggesters.Any();
        }

        public IEnumerable<DbTrack> SelectTracks( IPlayQueue playQueue, int minCount ) {
            if( StrategyHasBeenSet()) {
                var context = mContextFactory.CreateContext( playQueue );
                var circuitBreaker = 5;

                do {
                    mPlaySuggesters.ForEach( h => h.SelectTrack( context ));

                    if( context.SelectedTracks.Any()) {
                        mPlayDisqualifiers.ForEach( h => h.SelectTrack( context ));
                    }

                    if( context.SelectedTracks.Any()) {
                        mPlayBonusHandlers.ForEach( h => h.SelectTrack( context ));
                    }

                    circuitBreaker--;
                } while(( circuitBreaker > 0 ) &&
                        (!context.SelectedTracks.Any()));

                return context.SelectedTracks;
            }

            return new DbTrack[0];
        }
    }
}
