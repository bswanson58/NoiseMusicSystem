namespace ReusableBits.Mvvm.VersionSpinner {
    public interface IDigitIncrementer {
        void    SetBoundaries( int minimum, int maximum );
        void    SetTarget( int value );
        void    SetNextIncrementer( IDigitIncrementer next );

        int     CurrentValue {get; }

        void    IncrementCount();
        bool    HasReachedUpperBoundary();
    }
}
