namespace ReusableBits.Mvvm.VersionSpinner {
    public class DigitIncrementer : IDigitIncrementer {
        private int                 mMinimum;
        private int                 mMaximum;
        private int                 mTargetValue;
        public  int                 CurrentValue { get; private set; }
        private IDigitIncrementer   mNextIncrementer;

        public DigitIncrementer() {
            mMinimum = 0;
            mMaximum = 9;

            CurrentValue = 0;
        }

        public void SetBoundaries( int minimum, int maximum ) {
            mMinimum = minimum;
            mMaximum = maximum;

            CurrentValue = minimum;
        }

        public void SetTarget( int value ) {
            mTargetValue = value;
        }

        public void SetNextIncrementer( IDigitIncrementer next ) {
            mNextIncrementer = next;
        }

        public void IncrementCount() {
            if( CurrentValue >= mTargetValue ) {
                if( mNextIncrementer != null ) {
                    if(!mNextIncrementer.HasReachedUpperBoundary()) {
                        if( CurrentValue < mMaximum ) {
                            CurrentValue++;
                        }
                        else {
                            mNextIncrementer.IncrementCount();

                            CurrentValue = mMinimum;
                        }
                    }
                }
            }
            else {
                CurrentValue++;
            }
        }

        public bool HasReachedUpperBoundary() {
            var retValue = false;

            if( CurrentValue >= mTargetValue ) {
                retValue = mNextIncrementer == null || mNextIncrementer.HasReachedUpperBoundary();
            }

            return retValue;
        }
    }
}
