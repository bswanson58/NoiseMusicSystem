using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReusableBits.Ui.Controls {
	// inspired by: http://www.codeproject.com/Articles/136786/Creating-an-Animated-ContentControl

	// Example usage:
	//
	//	<Resources>
	//		<Storyboard x:Key="OutgoingStoryboard">
	//			<DoubleAnimation Storyboard.TargetProperty="Opacity" From="1" To="0" Duration="0:0:0.3"/>
	//		</Storyboard>
	//		
	//		<Storyboard x:Key="IncomingStoryboard">
	//			<DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Opacity">
	//				<LinearDoubleKeyFrame KeyTime="0:0:0" Value="0.0"/>
	//				<LinearDoubleKeyFrame KeyTime="0:0:0.3" Value="0.0"/>
	//				<LinearDoubleKeyFrame KeyTime="0:0:0.5" Value="1"/>
	//			</DoubleAnimationUsingKeyFrames>
	//		</Storyboard>
	//	</Resources>
	//
	//	<AnimatedContentControl Content="{Binding Content}"
	//							IncomingStoryboard="{StaticResource IncomingStoryboard}" IncomingRenderTransformOrigin="1.0, 0.5"
	//							OutgoingStoryboard="{StaticResource OutgoingStoryboard}" />
	//

	/// <summary>
	/// A ContentControl that animates the transition between content
	/// </summary>
	[TemplatePart( Name = "PART_PaintArea", Type = typeof( Shape )),
	 TemplatePart( Name = "PART_MainContent", Type = typeof( ContentPresenter ))]
	public class AnimatedContentControl : ContentControl {
		private	Shape				mPaintArea;
		private	ContentPresenter	mMainContent;

		static AnimatedContentControl() {
			DefaultStyleKeyProperty.OverrideMetadata( typeof( AnimatedContentControl ), new FrameworkPropertyMetadata( typeof( AnimatedContentControl )));
		}

		public static readonly DependencyProperty OutgoingStoryboardProperty = DependencyProperty.Register(
														"OutgoingStoryboard", typeof( Storyboard ), typeof( AnimatedContentControl ), new PropertyMetadata( null ));
		public Storyboard OutgoingStoryboard {
			get { return GetValue( OutgoingStoryboardProperty ) as Storyboard; }
			set { SetValue( OutgoingStoryboardProperty, value ); }
		}

		public static DependencyProperty OutgoingRenderTransformOriginProperty = DependencyProperty.Register(
														"OutgoingRenderTransformOrigin", typeof( Point ), typeof( AnimatedContentControl ), new PropertyMetadata( new Point( 0.5, 0.5 )));
		public Point OutgoingRenderTransformOrigin {
			get { return (Point)GetValue( OutgoingRenderTransformOriginProperty ); }
			set { SetValue( OutgoingRenderTransformOriginProperty, value ); }
		}

		public static readonly DependencyProperty IncomingStoryboardProperty = DependencyProperty.Register(
														"IncomingStoryboard", typeof( Storyboard ), typeof( AnimatedContentControl ), new PropertyMetadata( null ));
		public Storyboard IncomingStoryboard {
			get { return GetValue( IncomingStoryboardProperty ) as Storyboard; }
			set { SetValue( IncomingStoryboardProperty, value ); }
		}

		public static DependencyProperty IncomingRenderTransformOriginProperty = DependencyProperty.Register(
														"IncomingRenderTransformOrigin", typeof( Point ), typeof( AnimatedContentControl ), new PropertyMetadata( new Point( 0.5, 0.5 )));
		public Point IncomingRenderTransformOrigin {
			get { return (Point)GetValue( IncomingRenderTransformOriginProperty ); }
			set { SetValue( IncomingRenderTransformOriginProperty, value ); }
		}

		/// <summary>
		/// Called when the template has been applied and we have our visual tree
		/// </summary>
		public override void OnApplyTemplate() {
			mPaintArea = Template.FindName( "PART_PaintArea", this ) as Shape;
			mMainContent = Template.FindName( "PART_MainContent", this ) as ContentPresenter;

			if( mMainContent != null ) {
				mMainContent.ContentTemplateSelector = ContentTemplateSelector;
			}

			base.OnApplyTemplate();
		}

		/// <summary>
		/// Called when the content we're displaying has changed
		/// </summary>
		/// <param name="oldContent">The content that was previously displayed</param>
		/// <param name="newContent">The new content that is displayed</param>
		protected override void OnContentChanged( object oldContent, object newContent ) {
			if(( mPaintArea != null ) &&
			   ( mMainContent != null )) {
				BeginAnimateContentReplacement( oldContent != null, newContent != null );
			}

			base.OnContentChanged( oldContent, newContent );
		}

		/// <summary>
		/// Start the animation for the new content
		/// </summary>
		private void BeginAnimateContentReplacement( bool outgoing, bool incoming ) {
			if( outgoing ) {
				var storyboard = OutgoingStoryboard;

				if( storyboard != null ) {
					foreach( var timeline in storyboard.Children ) {
						Storyboard.SetTarget( timeline, mPaintArea );
					}

					mPaintArea.Fill = CreateBrushFromVisual( mMainContent );
					mPaintArea.Visibility = Visibility.Visible;

					storyboard.Completed += ( s, e ) => mPaintArea.Visibility = Visibility.Hidden;
					storyboard.Begin();
				}
			}

			if( incoming ) {
				var storyboard = IncomingStoryboard;

				if( storyboard != null ) {
					foreach( var timeline in storyboard.Children ) {
						Storyboard.SetTarget( timeline, mMainContent );
					}

					storyboard.Begin();
				}
			}
		}

		/// <summary>
		/// Creates a brush based on the current appearance of a visual element. The brush is an ImageBrush and once created, won't update its look
		/// </summary>
		/// <param name="visual">The visual element to take a snapshot of</param>
		private Brush CreateBrushFromVisual( Visual visual ) {
			if( visual == null ) {
				throw new ArgumentNullException( "visual" );
			}

			var target = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32 );
			target.Render( visual );

			var brush = new ImageBrush( target );
			brush.Freeze();

			return( brush );
		}
	}
}
