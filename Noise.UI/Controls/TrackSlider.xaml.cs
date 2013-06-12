using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for TrackSlider.xaml
	/// </summary>
	public partial class TrackSlider {
		private const int			cIntervalHistory = 5;

		private readonly Stopwatch	mTimer;
		private readonly List<long>	mPastIntervals;
		private long				mMinValue;
		private long				mMaxValue;
		private long				mValue;

		public TrackSlider() {
			InitializeComponent();

			mTimer = new Stopwatch();
			mPastIntervals = new List<long>();

			mMinValue = 0;
			mMaxValue = 100;
			mValue = 0;
		}

		// Before Property - used to control extent of past slider segment.
		public static DependencyProperty BeforeProperty = DependencyProperty.Register( "Before", typeof( GridLength ), typeof( TrackSlider ),
																							new PropertyMetadata( new GridLength( 0, GridUnitType.Star )));
		public GridLength Before {
			get { return (GridLength)GetValue( BeforeProperty ); }
			set { SetValue( BeforeProperty, value ); }
		}

		// BeforeTemplate property - specify the look of the past segment.
		public static DependencyProperty BeforeTemplateProperty = DependencyProperty.Register( "BeforeTemplate", typeof( DataTemplate ), typeof( TrackSlider ),
																							new PropertyMetadata( null ) );
		public DataTemplate BeforeTemplate {
			get { return (DataTemplate)GetValue( BeforeTemplateProperty ); }
			set { SetValue( BeforeTemplateProperty, value ); }
		}

		// After property - used to control the extent of the future slider segment.
		public static DependencyProperty AfterProperty = DependencyProperty.Register( "After", typeof( GridLength ), typeof( TrackSlider ),
																							new PropertyMetadata( new GridLength( 1, GridUnitType.Star )));
		public GridLength After {
			get { return (GridLength)GetValue( AfterProperty ); }
			set { SetValue( AfterProperty, value ); }
		}

		// AfterTemplate property - specify the look of the future segment.
		public static DependencyProperty AfterTemplateProperty = DependencyProperty.Register( "AfterTemplate", typeof( DataTemplate ), typeof( TrackSlider ),
																							new PropertyMetadata( null ) );
		public DataTemplate AfterTemplate {
			get { return (DataTemplate)GetValue( AfterTemplateProperty ); }
			set { SetValue( AfterTemplateProperty, value ); }
		}

		// ThumbTemplate property - specify the look of the thumb segment.
		public static DependencyProperty ThumbTemplateProperty = DependencyProperty.Register( "ThumbTemplate", typeof( DataTemplate ), typeof( TrackSlider ),
																							new PropertyMetadata( null ));
		public DataTemplate ThumbTemplate {
			get { return (DataTemplate)GetValue( ThumbTemplateProperty ); }
			set { SetValue( ThumbTemplateProperty, value ); }
		}
		// Minimum property.
		public static DependencyProperty MinimumProperty = DependencyProperty.Register( "Minimum", typeof( long ), typeof( TrackSlider ),
															new PropertyMetadata( 0L, OnMinimumChanged, OnCoerceMinimumValue ));
		public long Minimum {
			get { return ( (long)GetValue( MinimumProperty ) ); }
			set { SetValue( MinimumProperty, value ); }
		}
		private static void OnMinimumChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is TrackSlider ) {
				var slider = sender as TrackSlider;

				slider.UpdateMinimumValue( (long)args.NewValue );
			}
		}
		private static object OnCoerceMinimumValue( DependencyObject sender, object value ) {
			var retValue = value;

			if(( value is long ) &&
			   ( sender is TrackSlider )) {
				var slider = sender as TrackSlider;
				var minimum = (long)value;

				if( minimum > slider.Maximum ) {
					retValue = slider.Maximum;
				}

			}

			return ( retValue );
		}

		// Maximum property.
		public static DependencyProperty MaximumProperty = DependencyProperty.Register( "Maximum", typeof( long ), typeof( TrackSlider ),
															new PropertyMetadata( 100L, OnMaximumChanged, OnCoerceMaximumValue ));
		public long Maximum {
			get { return ((long)GetValue( MaximumProperty )); }
			set { SetValue( MaximumProperty, value ); }
		}
		private static void OnMaximumChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is TrackSlider ) {
				var slider = sender as TrackSlider;

				slider.UpdateMaximumValue( (long)args.NewValue );
			}
		}
		private static object OnCoerceMaximumValue( DependencyObject sender, object value ) {
			var retValue = value;

			if(( value is long ) &&
			   ( sender is TrackSlider )) {
				var slider = sender as TrackSlider;
				var maximum = (long)value;

				if( maximum < slider.Minimum ) {
					retValue = slider.Minimum;
				}

			}

			return ( retValue );
		}

		// Value property
		public static DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( long ), typeof( TrackSlider ),
															new PropertyMetadata( 0L, OnValueChanged, OnCoerceValue ));
		public long Value {
			get { return ((long)GetValue( ValueProperty )); }
			set { SetValue( ValueProperty, value ); }
		}
		private static void OnValueChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is TrackSlider ) {
				var slider = sender as TrackSlider;

				slider.UpdateValue( (long)args.NewValue );
			}
		}
		private static object OnCoerceValue( DependencyObject sender, object value ) {
			var retValue = value;

			if(( value is long ) &&
			   ( sender is TrackSlider )) {
				var slider = sender as TrackSlider;
				var newValue = (long)value;

				if( newValue > slider.Maximum ) {
					retValue = slider.Maximum;
				}
				if( newValue < slider.Minimum ) {
					retValue = slider.Minimum;
				}

			}

			return ( retValue );
		}

		// SmoothedValue property
		public static DependencyProperty SmoothedValueProperty = DependencyProperty.Register( "SmoothedValue", typeof( long ), typeof( TrackSlider ),
																			new PropertyMetadata( 0L, OnSmoothedValueChanged, OnCoerceValue ));
		public long SmoothedValue {
			get { return ( (long)GetValue( SmoothedValueProperty )); }
			set { SetValue( SmoothedValueProperty, value ); }
		}
		private static void OnSmoothedValueChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is TrackSlider ) {
				var slider = sender as TrackSlider;

				slider.UpdateSmoothedValue( (long)args.NewValue );
			}
		}

		private void UpdateMinimumValue( long value ) {
			mMinValue = value;

			UpdatePosition();
		}

		private void UpdateMaximumValue( long value ) {
			mMaxValue = value;

			UpdatePosition();
		}

		private void UpdateValue( long value ) {
			mValue = value;

			UpdatePosition();
		}

		private void UpdateSmoothedValue( long value ) {
			if( mTimer.IsRunning ) {
				mPastIntervals.Add( mTimer.ElapsedMilliseconds );
			}
			mTimer.Restart();

			if( mPastIntervals.Any()) {
				UpdatePosition( value, (long)mPastIntervals.Average());

				if( mPastIntervals.Count > cIntervalHistory ) {
					mPastIntervals.RemoveRange( 0, mPastIntervals.Count - cIntervalHistory );
				}
			}
			else {
				mValue = value;

				UpdatePosition();
			}
		}

		private void UpdatePosition() {
			var range = (double)mMaxValue - mMinValue;
			var position = (double)mValue - mMinValue;

			if(( mValue >= mMinValue ) &&
			   ( mValue <= mMaxValue )) {
				var	placement = position / range;

				Before = new GridLength( placement, GridUnitType.Star );
				After = new GridLength( 1.0 - placement, GridUnitType.Star );
			}
		}

		private void UpdatePosition( long value, long ms ) {
			var animation = new Int64Animation { From = mValue, To = value, Duration = new Duration( new TimeSpan( 0, 0, 0, 0, (int)ms )) };
			var storyboard = new Storyboard();

			storyboard.Children.Add( animation );
			Storyboard.SetTarget( animation, this );
			Storyboard.SetTargetProperty( animation, new PropertyPath( ValueProperty ));

			storyboard.Begin( this );
		}
	}
}
