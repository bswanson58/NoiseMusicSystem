using System;

namespace MilkBottle.Support {
    class LimitedRepeatingRandom {
        private readonly Random     mRandom;
        private readonly Double     mRepeatFactor;
        private LimitedStack<int>   mPriorResults;
        private int                 mMaximumResult;

        public LimitedRepeatingRandom( double repeatFactor ) {
            mRepeatFactor = Math.Max( 0.0, Math.Min( 1.0, repeatFactor ));

            mRandom = new Random( DateTime.Now.Millisecond );
        }

        public int Next( int minValue, int maxValue ) {
            return Next( maxValue - minValue ) + minValue;
        }

        public int Next( int maxValue ) {
            if( maxValue != mMaximumResult ) {
                Initialize( maxValue );
            }

            var retValue = GenerateRandom();

            while( mPriorResults.Contains( retValue )) {
                retValue = GenerateRandom();
            }

            mPriorResults.Push( retValue );

            return retValue;
        }

        private void Initialize( int maxValue ) {
            var repeatSize = Math.Max( 1, (int)( mRepeatFactor * maxValue  ));

            mMaximumResult = maxValue;
            mPriorResults = new LimitedStack<int>( repeatSize );
        }

        private int GenerateRandom() {
            return mRandom.Next( mMaximumResult );
        }
    }
}
