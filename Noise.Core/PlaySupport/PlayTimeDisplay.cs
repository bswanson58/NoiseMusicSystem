using System.Collections.Generic;

namespace Noise.Core.PlaySupport {
    internal class PlayTimeDisplay {
        private const int		Left	= 0b1000;
        private const int		LeftUp	= 0b0100;
        private const int		Right	= 0b0010;
        private const int		RightUp	= 0b0001;

        private readonly List<int>	mValidPlayValues;
        private int					mPlayIndex;

        public	int					CurrentMode { get; private set; }
        public	bool				IsLeftDisplayed => ( CurrentMode & Left ) == Left;
        public	bool				IsLeftIncrementing => (CurrentMode & LeftUp ) == LeftUp;
        public	bool				IsRightDisplayed => ( CurrentMode & Right ) == Right;
        public	bool				IsRightIncrementing => ( CurrentMode & RightUp ) == RightUp;

        public PlayTimeDisplay() {
            mValidPlayValues = new List<int> { Left, Left + LeftUp, Right, Right + RightUp, Left + LeftUp + Right };
        }

        public void SetMode( int mode ) {
            if( mValidPlayValues.Contains( mode )) {
                mPlayIndex = mValidPlayValues.IndexOf( mode );
                CurrentMode = mValidPlayValues[mPlayIndex];
            }
            else {
                mPlayIndex = 0;
                CurrentMode = mValidPlayValues[mPlayIndex];
            }
        }

        public void SetNextMode() {
            mPlayIndex++;

            if( mPlayIndex >= mValidPlayValues.Count ) {
                mPlayIndex = 0;
            }

            CurrentMode = mValidPlayValues[mPlayIndex];
        }
    }
}
