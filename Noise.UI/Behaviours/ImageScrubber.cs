using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Noise.UI.Behaviours {
	public class ImageScrubberItem {
		public long			Id { get; private set; }
		public BitmapImage	Image { get; private set; }
		public int			Rotation { get; private set; }
		public double		ImageLeft { get; private set; }
		public double		ImageTop { get; private set; }

		public ImageScrubberItem( long id, BitmapImage image, int rotation ) {
			Id = id;
			Image = image;
			Rotation = rotation;

			if(( Rotation == 90 ) ||
			   ( Rotation == 270 )) {
				ImageTop = ( Image.Width - Image.Height ) / 2;
				ImageLeft = ( Image.Height - Image.Width ) / 2;
			}
			else {
				ImageTop = 0.0;
				ImageLeft = 0.0;
			}
		}
	}

	public class ImageScrubber : System.Windows.Interactivity.Behavior<Image> {
		private Image				mImage;
		private bool				mScrubbing;
		private	Point				mStartingPosition;
		private ImageScrubberItem	mCurrentImage;
		private int					mImageIndex;

		public static readonly DependencyProperty ImageListProperty =
				DependencyProperty.RegisterAttached( "ImageList",
													 typeof( IEnumerable<ImageScrubberItem> ),
													 typeof( ImageScrubber ),
													 new FrameworkPropertyMetadata( null, OnImageListChanged ));

		public IEnumerable<ImageScrubberItem> ImageList {
			get { return ( (IEnumerable<ImageScrubberItem>)GetValue( ImageListProperty )); }
			set { SetValue( ImageListProperty, value ); }
		}

		public static readonly DependencyProperty ImageProperty =
				DependencyProperty.RegisterAttached( "Image",
													 typeof( ImageScrubberItem ),
													 typeof( ImageScrubber ),
													 new FrameworkPropertyMetadata( null, OnImageChanged ));

		public ImageScrubberItem Image {
			get { return ((ImageScrubberItem)GetValue( ImageProperty )); }
			set { SetValue( ImageProperty, value ); }
		}

		private static void OnImageChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if(( depObj is ImageScrubber ) &&
			   ( args.NewValue is ImageScrubberItem )) {
				( depObj as ImageScrubber ).SetTargetImage( args.NewValue as ImageScrubberItem );
			}
		}

		private static void OnImageListChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if(( depObj is ImageScrubber ) &&
			   ( args.NewValue is ImageScrubberItem )) {
			}
		}

		public void SetTargetImage( ImageScrubberItem value ) {
			if( mImage != null ) {
				mImage.Source = value.Image;
				mCurrentImage = value;
			}
		}

		protected override void OnAttached() {
			base.OnAttached();

			mImage = AssociatedObject;
			if( mImage != null ) {
				mImage.MouseLeftButtonDown += OnMouseDown;
				mImage.MouseLeftButtonUp += OnMouseUp;
				mImage.MouseMove += OnMouseMove;
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if( mImage != null ) {
				mImage.MouseLeftButtonDown -= OnMouseDown;
				mImage.MouseLeftButtonUp -= OnMouseUp;
				mImage.MouseMove -= OnMouseMove;

				mImage = null;
			}
		}

		private void SelectItem( bool advance ) {
			if(( mImageIndex < 0 ) ||
			   ( mImageIndex >= ImageList.Count())) {
				mImageIndex = 0;
			}

			if( advance ) {
				mImageIndex = mImageIndex < ( ImageList.Count() - 1 ) ? mImageIndex + 1 : mImageIndex;
			}
			else {
				mImageIndex = mImageIndex > 0 ? mImageIndex - 1 : 0;
			}

			SetTargetImage( ImageList.ElementAt( mImageIndex ));
		}

		private void OnMouseDown( object sender, MouseButtonEventArgs args ) {
			if( ImageList.Any()) {
				mScrubbing = true;
				mStartingPosition = Mouse.GetPosition( null );

				mImage.CaptureMouse();
			}
		}

		private void OnMouseMove( object sender, MouseEventArgs args ) {
			if( mScrubbing ) {
				var currentPostion = Mouse.GetPosition( null );
				var delta = currentPostion.X - mStartingPosition.X;

				if( Math.Abs( delta ) > ( SystemParameters.MinimumHorizontalDragDistance * 10.0 )) {
					mStartingPosition = currentPostion;
					SelectItem( delta > 0 );
				}
			}
		}

		private void OnMouseUp( object sender, MouseButtonEventArgs args ) {
			mScrubbing = false;
			mImage.ReleaseMouseCapture();

			if( mCurrentImage != null ) {
				Image = mCurrentImage;
			}
		}
	}
}
