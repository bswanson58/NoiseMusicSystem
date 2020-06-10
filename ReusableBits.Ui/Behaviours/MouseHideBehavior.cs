using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

namespace ReusableBits.Ui.Behaviours {
	// usage: ( Timeout is in seconds )
	//<Window ... >
    //<i:Interaction.Behaviors>
    //    <Behaviours:MouseHideBehavior Timeout="3.0" />
    //</i:Interaction.Behaviors>
	//</Window>
	public class MouseHideBehavior : Behavior<Window> {
		private readonly DispatcherTimer	mTimer;
		private long						mLastMoveTicks;
		private Point						mLastMousePosition;
		private bool						mCursorHidden;
		private Cursor						mLastCursor;

		public static readonly DependencyProperty TimeoutProperty = DependencyProperty.Register( "Timeout",
									typeof( double ), typeof( MouseHideBehavior ), new PropertyMetadata( 3.0D, OnTimeoutChanged ));

		public double Timeout {
			get { return (double)GetValue( TimeoutProperty ); }
			set { SetValue( TimeoutProperty, value ); }
		}

		private static void OnTimeoutChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is MouseHideBehavior ) {
				var behavior = sender as MouseHideBehavior;

				behavior.UpdateTimer();
			}
		}

		public MouseHideBehavior() {
			mTimer = new DispatcherTimer();
			mLastMousePosition = new Point();

			UpdateTimer();
			mTimer.Tick += OnTimer;
		}

		private void OnTimer( object sender, EventArgs args ) {
			if((( DateTime.Now - TimeSpan.FromSeconds( Timeout )).Ticks ) > mLastMoveTicks ) {
				mTimer.Stop();

				mLastCursor = AssociatedObject.Cursor;
				mCursorHidden = true;

				AssociatedObject.Cursor = Cursors.None;
			}
		}

		private void UpdateTimer() {
			mTimer.Interval = TimeSpan.FromSeconds( Timeout );
		}

		private void OnMouseMove( object sender, MouseEventArgs args ) {
			var position = args.GetPosition( AssociatedObject );

			if( mLastMousePosition != position ) {
				mLastMoveTicks = DateTime.Now.Ticks;
				mLastMousePosition = position;

				if( mCursorHidden ) {
					AssociatedObject.Cursor = mLastCursor;

					mCursorHidden = false;
					mTimer.Start();
				}
			}
		}

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.MouseMove += OnMouseMove;

			UpdateTimer();
			mTimer.Start();
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			mTimer.Stop();

			AssociatedObject.MouseMove -= OnMouseMove;
		}
	}
}
