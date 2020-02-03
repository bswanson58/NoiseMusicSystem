using System;
using System.Timers;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.VersionSpinner {
    public enum VersionLevel {
        Major,
        Minor,
        Build,
        Revision
    }

    public class VersionSpinnerViewModel : PropertyChangeBase, IVersionFormatter {
        private readonly IDigitIncrementer  mMajorCounter;
        private readonly IDigitIncrementer  mMinorCounter;
        private readonly IDigitIncrementer  mBuildCounter;
        private readonly IDigitIncrementer  mRevisionCounter;
        private readonly Timer              mTimer;
        private VersionLevel                mDisplayLevel;

        public  int                         Major => mMajorCounter.CurrentValue;
        public  bool                        DisplayMajor => true;
        public  int                         Minor => mMinorCounter.CurrentValue;
        public  bool                        DisplayMinor => DisplayLevel == VersionLevel.Revision || DisplayLevel == VersionLevel.Build || DisplayLevel == VersionLevel.Minor;
        public  int                         Build => mBuildCounter.CurrentValue;
        public  bool                        DisplayBuild => DisplayLevel == VersionLevel.Revision || DisplayLevel == VersionLevel.Build;
        public  int                         Revision => mRevisionCounter.CurrentValue;
        public  bool                        DisplayRevision => DisplayLevel == VersionLevel.Revision;

        public VersionSpinnerViewModel() {
            mMajorCounter = new DigitIncrementer();
            mMinorCounter = new DigitIncrementer();
            mBuildCounter = new DigitIncrementer();
            mRevisionCounter = new DigitIncrementer();

            mTimer = new Timer();
            mTimer.Elapsed += OnTimerElapsed;

//            mMinorCounter.SetNextIncrementer( mMajorCounter );
//            mBuildCounter.SetNextIncrementer( mMinorCounter );
//            mRevisionCounter.SetNextIncrementer( mBuildCounter );
        }

        public void SetVersion( Version version ) {
            mMajorCounter.SetTarget( version.Major );
            mMajorCounter.SetBoundaries( 0, Math.Max( 10, version.Major ));
            mMinorCounter.SetTarget( version.Minor );
            mMinorCounter.SetBoundaries( 0, Math.Max( 10, version.Minor ));
            mBuildCounter.SetTarget( version.Build );
            mBuildCounter.SetBoundaries( 0, Math.Max( 10, version.Build ));
            mRevisionCounter.SetTarget( version.Revision );
            mRevisionCounter.SetBoundaries( 0, Math.Max( 10, version.Revision ));
        }

        public void StartFormatting() {
            mTimer.Interval = 150;
            mTimer.Start();
        }

        private void OnTimerElapsed( object sender, ElapsedEventArgs elapsedEventArgs ) {
            IncrementCounters();

            if( HasReachedUpperBoundary()) {
                mTimer.Stop();
            }

            RaisePropertyChanged( () => Major );
            RaisePropertyChanged( () => Minor );
            RaisePropertyChanged( () => Build );
            RaisePropertyChanged( () => Revision );

            RaisePropertyChanged( () => VersionString );
        }

        private void IncrementCounters() {
            if(!mRevisionCounter.HasReachedUpperBoundary()) {
                mRevisionCounter.IncrementCount();
            }
            else if(!mBuildCounter.HasReachedUpperBoundary()) {
                mBuildCounter.IncrementCount();
            }
            else if(!mMinorCounter.HasReachedUpperBoundary()) {
                mMinorCounter.IncrementCount();
            }
            else {
                if(!mMajorCounter.HasReachedUpperBoundary()) {
                    mMajorCounter.IncrementCount();
                }
            }
        }

        private bool HasReachedUpperBoundary() {
            return mRevisionCounter.HasReachedUpperBoundary() && 
                   mBuildCounter.HasReachedUpperBoundary() && 
                   mMinorCounter.HasReachedUpperBoundary() && 
                   mMajorCounter.HasReachedUpperBoundary();
        }

        public void Dispose() {
            mTimer?.Dispose();
        }

        public VersionLevel DisplayLevel {
            get => mDisplayLevel;
            set {
                mDisplayLevel = value;

                RaisePropertyChanged( () => DisplayRevision );
                RaisePropertyChanged( () => DisplayBuild);
                RaisePropertyChanged( () => DisplayMinor );
                RaisePropertyChanged( () => DisplayMajor );
            }
        }
        public string VersionString {
            get {
                var retValue = String.Empty;

                switch( DisplayLevel ) {
                    case VersionLevel.Revision:
                        retValue = $"{Major}.{Minor}.{Build}.{Revision}";
                        break;
                    case VersionLevel.Build:
                        retValue = $"{Major}.{Minor}.{Build}";
                        break;
                    case VersionLevel.Minor:
                        retValue = $"{Major}.{Minor}";
                        break;
                    case VersionLevel.Major:
                        retValue = $"{Major}";
                        break;
                }

                return retValue;
            }
        }
    }
}
