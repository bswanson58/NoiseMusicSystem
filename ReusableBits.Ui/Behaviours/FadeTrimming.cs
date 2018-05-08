using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

// from: http://blog.hibernatingrhinos.com/12385/fade-trimming-textblocks-in-silverlight-and-wpf
// source: https://github.com/samueldjack/FadeTrimming

namespace ReusableBits.Ui.Behaviours {
	public static class FadeTrimming {
		private	const double cEpsilon = 0.00001;
		private const double cFadeWidth = 35.0;
		private const double cFadeHeight = 20.0;

		public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( FadeTrimming ), new PropertyMetadata( false, HandleIsEnabledChanged ) );

		public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.RegisterAttached( "ForegroundColor", typeof( Color ), typeof( FadeTrimming ), new PropertyMetadata( Colors.Transparent ) );

		public static readonly DependencyProperty ShowTextInToolTipWhenTrimmedProperty =
            DependencyProperty.RegisterAttached( "ShowTextInToolTipWhenTrimmed", typeof( bool ), typeof( FadeTrimming ), new PropertyMetadata( false ) );

		private static readonly DependencyProperty FaderProperty =
            DependencyProperty.RegisterAttached( "Fader", typeof( Fader ), typeof( FadeTrimming ), new PropertyMetadata( null ) );

		public static bool GetIsEnabled( DependencyObject obj ) {
			return (bool)obj.GetValue( IsEnabledProperty );
		}

		public static void SetIsEnabled( DependencyObject obj, bool value ) {
			obj.SetValue( IsEnabledProperty, value );
		}

		public static bool GetShowTextInToolTipWhenTrimmed( DependencyObject obj ) {
			return (bool)obj.GetValue( ShowTextInToolTipWhenTrimmedProperty );
		}

		public static void SetShowTextInToolTipWhenTrimmed( DependencyObject obj, bool value ) {
			obj.SetValue( ShowTextInToolTipWhenTrimmedProperty, value );
		}

		public static Color GetForegroundColor( DependencyObject obj ) {
			return (Color)obj.GetValue( ForegroundColorProperty );
		}

		public static void SetForegroundColor( DependencyObject obj, Color value ) {
			obj.SetValue( ForegroundColorProperty, value );
		}

		private static Fader GetFader( DependencyObject obj ) {
			return (Fader)obj.GetValue( FaderProperty );
		}

		private static void SetFader( DependencyObject obj, Fader value ) {
			obj.SetValue( FaderProperty, value );
		}

		private static void HandleIsEnabledChanged( DependencyObject source, DependencyPropertyChangedEventArgs e ) {
			var textBlock = source as TextBlock;
			if( textBlock == null ) {
				return;
			}

			if( (bool)e.OldValue ) {
				var fader = GetFader( textBlock );
				if( fader != null ) {
					fader.Detach();
					SetFader( textBlock, null );
				}

				textBlock.Loaded -= HandleTextBlockLoaded;
				textBlock.Unloaded -= HandleTextBlockUnloaded;
			}

			if( (bool)e.NewValue ) {
				textBlock.Loaded += HandleTextBlockLoaded;
				textBlock.Unloaded += HandleTextBlockUnloaded;

				var fader = new Fader( textBlock );
				SetFader( textBlock, fader );
				fader.Attach();
			}
		}

		private static void HandleTextBlockUnloaded( object sender, RoutedEventArgs e ) {
			var fader = GetFader( sender as DependencyObject );
			fader.Detach();
		}

		private static void HandleTextBlockLoaded( object sender, RoutedEventArgs e ) {
			var fader = GetFader( sender as DependencyObject );
			fader.Attach();
		}

		private class Fader {
			private readonly TextBlock	mTextBlock;
			private bool				mIsAttached;
			private LinearGradientBrush	mBrush;
			private Color				mForegroundColor;
			private bool				mIsClipped;

			public Fader( TextBlock textBlock ) {
				mTextBlock = textBlock;
			}

			public void Attach() {
				var parent = VisualTreeHelper.GetParent( mTextBlock ) as FrameworkElement;
				if( parent == null || mIsAttached ) {
					return;
				}

				parent.SizeChanged += UpdateForegroundBrush;
				mTextBlock.SizeChanged += UpdateForegroundBrush;

				mForegroundColor = DetermineForegroundColor( mTextBlock );
				UpdateForegroundBrush( mTextBlock, EventArgs.Empty );

				mIsAttached = true;
			}

			public void Detach() {
				mTextBlock.SizeChanged -= UpdateForegroundBrush;

				var parent = VisualTreeHelper.GetParent( mTextBlock ) as FrameworkElement;
				if( parent != null ) {
					parent.SizeChanged -= UpdateForegroundBrush;
				}

				// remove our explicitly set Foreground color
				mTextBlock.ClearValue( TextBlock.ForegroundProperty );
				mIsAttached = false;
			}

			private Color DetermineForegroundColor( TextBlock textBlock ) {
				// if our own Attached Property has been used to set an explicit foreground color, use that
				if( GetForegroundColor( textBlock ) != Colors.Transparent ) {
				    mTextBlock.Foreground = new SolidColorBrush { Color = GetForegroundColor( textBlock ) };

					return GetForegroundColor( textBlock );
				}

				// otherwise, if the textBlock has inherited a foreground color, use that
				if( textBlock.Foreground is SolidColorBrush ) {
					return ( textBlock.Foreground as SolidColorBrush ).Color;
				}

				return Colors.Black;
			}

			private void UpdateForegroundBrush( object sender, EventArgs e ) {
				// determine if the TextBlock has been clipped
				var layoutClip = LayoutInformation.GetLayoutClip( mTextBlock );

				bool needsClipping = layoutClip != null
					&& ( ( mTextBlock.TextWrapping == TextWrapping.NoWrap && layoutClip.Bounds.Width > 0
					&& layoutClip.Bounds.Width < mTextBlock.ActualWidth )
					|| ( mTextBlock.TextWrapping == TextWrapping.Wrap && layoutClip.Bounds.Height > 0
					&& layoutClip.Bounds.Height < mTextBlock.ActualHeight ) );

				// if the TextBlock was clipped, but is no longer clipped, then
				// strip all the fancy features
				if( mIsClipped && !needsClipping ) {
					if( GetShowTextInToolTipWhenTrimmed( mTextBlock ) ) {
						mTextBlock.ClearValue( ToolTipService.ToolTipProperty );
					}

					mTextBlock.Foreground = new SolidColorBrush { Color = mForegroundColor };
					mBrush = null;
					mIsClipped = false;
				}

				// if the TextBlock has just become clipped, make its
				// content show in its tooltip
				if( !mIsClipped && needsClipping ) {
					if( GetShowTextInToolTipWhenTrimmed( mTextBlock ) ) {
						BindingOperations.SetBinding( mTextBlock, ToolTipService.ToolTipProperty,
													 new Binding( "Text" ) { Source = mTextBlock } );
					}
				}

				// here's the real magic: if the TextBlock is clipped
				// update its Foreground brush to make it fade out just
				// inside the clip boundary
				if( needsClipping ) {
					var visibleWidth = layoutClip.Bounds.Width;
					var visibleHeight = layoutClip.Bounds.Height;

					var verticalClip = mTextBlock.TextWrapping == TextWrapping.Wrap;

					if( mBrush == null ) {
						mBrush = verticalClip ? GetVerticalClipBrush( visibleHeight ) : GetHorizontalClipBrush( visibleWidth );
						mTextBlock.Foreground = mBrush;
					}
					else if( verticalClip && VerticalBrushNeedsUpdating( mBrush, visibleHeight ) ) {
						mBrush.EndPoint = new Point( 0, visibleHeight );
						mBrush.GradientStops[1].Offset = ( visibleHeight - cFadeHeight ) / visibleHeight;
					}
					else if( !verticalClip && HorizontalBrushNeedsUpdating( mBrush, visibleWidth ) ) {
						mBrush.EndPoint = new Point( visibleWidth, 0 );
						mBrush.GradientStops[1].Offset = ( visibleWidth - cFadeWidth ) / visibleWidth;
					}

					mIsClipped = true;
				}
			}

			private LinearGradientBrush GetHorizontalClipBrush( double visibleWidth ) {
				return new LinearGradientBrush {
					// set MappingMode to absolute so that
					// we can specify the EndPoint of the brush in
					// terms of the TextBlock's actual dimensions
					MappingMode = BrushMappingMode.Absolute,
					StartPoint = new Point( 0, 0 ),
					EndPoint = new Point( visibleWidth, 0 ),
					GradientStops =
                                   {
                                       new GradientStop {Color = mForegroundColor, Offset = 0},
                                       new GradientStop {
                                               Color = mForegroundColor,
                                               // Even though the mapping mode is absolute,
                                               // the offset for gradient stops is always relative with
                                               // 0 being the start of the brush, and 1 the end of the brush
                                               Offset = (visibleWidth - cFadeWidth)/visibleWidth
                                           },
                                       new GradientStop {
                                               Color = Color.FromArgb(0, mForegroundColor.R, mForegroundColor.G, mForegroundColor.B),
                                               Offset = 1
                                           }
                                   }
				};
			}

			private LinearGradientBrush GetVerticalClipBrush( double visibleHeight ) {
				return new LinearGradientBrush {
					// set MappingMode to absolute so that
					// we can specify the EndPoint of the brush in
					// terms of the TextBlock's actual dimensions
					MappingMode = BrushMappingMode.Absolute,
					StartPoint = new Point( 0, 0 ),
					EndPoint = new Point( 0, visibleHeight ),
					GradientStops =
                                   {
                                       new GradientStop {Color = mForegroundColor, Offset = 0},
                                       new GradientStop {
                                               Color = mForegroundColor,
                                               // Even though the mapping mode is absolute,
                                               // the offset for gradient stops is always relative with
                                               // 0 being the start of the brush, and 1 the end of the brush
                                               Offset = (visibleHeight - cFadeHeight)/visibleHeight
                                           },
                                       new GradientStop {
                                               Color = Color.FromArgb(0, mForegroundColor.R, mForegroundColor.G, mForegroundColor.B),
                                               Offset = 1
                                           }
                                   }
				};
			}
		}

		private static bool HorizontalBrushNeedsUpdating( LinearGradientBrush brush, double visibleWidth ) {
			return brush.EndPoint.X < visibleWidth - cEpsilon || brush.EndPoint.X > visibleWidth + cEpsilon;
		}

		private static bool VerticalBrushNeedsUpdating( LinearGradientBrush brush, double visibleHeight ) {
			return brush.EndPoint.Y < visibleHeight - cEpsilon || brush.EndPoint.Y > visibleHeight + cEpsilon;
		}
	}
}
