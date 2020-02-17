using System;

namespace MilkBottle.Support {
    class LimitedRepeatingRandom {
        private readonly Random     mRandom;
        private readonly Double     mRepeatFactor;
        private LimitedStack<int>   mPriorResults;
        private int                 mMaximumResult;

        public LimitedRepeatingRandom( double repeatFactor ) {
            mRepeatFactor = Math.Max( 0.0, Math.Min( 0.8, repeatFactor ));

            mRandom = new Random( DateTime.Now.Millisecond );
        }

        public int Next( int minValue, int maxValue ) {
            return Next( maxValue - minValue ) + minValue;
        }

        public int Next( int maxValue ) {
            var retValue = 0;

            if( maxValue > 1 ) {
                if( maxValue != mMaximumResult ) {
                    Initialize( maxValue );
                }

                retValue = GenerateRandom();

                while( mPriorResults.Contains( retValue )) {
                    retValue = GenerateRandom();
                }

                mPriorResults.Push( retValue );
            }

            return retValue;
        }

        private void Initialize( int maxValue ) {
            var repeatSize = Math.Max( 1, (int)( mRepeatFactor * maxValue  ));

            mMaximumResult = maxValue;

            if( repeatSize >= mMaximumResult ) {
                repeatSize = Math.Max( 0, mMaximumResult );
            }

            mPriorResults = new LimitedStack<int>( repeatSize );
        }

        private int GenerateRandom() {
            return mRandom.Next( mMaximumResult );
        }
    }
}
