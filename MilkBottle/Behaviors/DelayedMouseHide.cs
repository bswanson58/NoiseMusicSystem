using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace MilkBottle.Behaviors {
    //<i:Interaction.Behaviors>
    //    <behaviors:DelayedMouseHide DelayPeriod="5"/>
    //</i:Interaction.Behaviors>

    class DelayedMouseHide : Behavior<FrameworkElement> {
        private readonly DispatcherTimer    mTimer;
        private Cursor                      mPreviousCursor;

        public static readonly DependencyProperty DelayPeriodProperty = DependencyProperty.Register(
            "DelayPeriod",
            typeof( int ),
            typeof( MetroWindowTitleColor ),
            new PropertyMetadata( 3 ));

        public int DelayPeriod {
            get => (int)GetValue( DelayPeriodProperty );
            set => SetValue( DelayPeriodProperty, value );
        }

        public DelayedMouseHide() {
            mTimer = new DispatcherTimer();
            mTimer.Tick += OnTimer;
            mTimer.Stop();
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        private void OnMouseEnter( object sender, MouseEventArgs args ) {
        }

        private void OnMouseLeave( object sender, MouseEventArgs args ) {
            mTimer.Stop();
        }

        private void OnMouseMove( object sender, MouseEventArgs args ) {
            DisplayCursor();
            StartTimer();
        }

        private void OnTimer( object sender, EventArgs args ) {
            HideCursor();
        }

        private void StartTimer() {
            mTimer.Stop();
            mTimer.Interval = TimeSpan.FromSeconds( DelayPeriod );
            mTimer.Start();
        }

        private void HideCursor() {
            mPreviousCursor = AssociatedObject.Cursor;

            AssociatedObject.Cursor = Cursors.None;
        }

        private void DisplayCursor() {
            AssociatedObject.Cursor = mPreviousCursor != Cursors.None ? mPreviousCursor : Cursors.Arrow;
        }
    }
}
