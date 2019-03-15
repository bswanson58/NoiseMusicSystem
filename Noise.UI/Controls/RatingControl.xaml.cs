using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Noise.UI.Controls {
	internal enum HoverPart {
		None,
		Dislike,
		Clear,
		Rating
	}

	/// <summary>
	/// Interaction logic for RatingControl.xaml
	/// </summary>
	public partial class RatingControl : INotifyPropertyChanged {
		private const int	cInitialRatingCount		= 5;
		private const int	cRatingDislike			= -1;
		private const int	cRatingUnrated			= 0;

		private Brush					mDislikePartColor;
		private Brush					mClearPartColor;
		private HoverPart				mHoverPart;
		private static readonly bool	IsInDesignMode;
		private readonly ObservableCollection<UiRatingItem>	mRatingItems;

		public	event PropertyChangedEventHandler		PropertyChanged;

		static RatingControl() {
#if SILVERLIGHT
			IsInDesignMode = DesignerProperties.IsInDesignTool;
#else
			var prop = DesignerProperties.IsInDesignModeProperty;
			IsInDesignMode
					= (bool)DependencyPropertyDescriptor
					.FromProperty( prop, typeof( FrameworkElement ) )
					.Metadata.DefaultValue;
#endif
		}

		public RatingControl() {
			InitializeComponent();

			mRatingItems = new ObservableCollection<UiRatingItem>();
			SetRatingCount( cInitialRatingCount );

			PART_LayoutRoot.DataContext = this;
			PART_LayoutRoot.MouseLeave += OnRootMouseLeave;

			PART_Dislike.MouseEnter += OnDislikeMouseEnter;
			PART_Dislike.MouseDown += OnDislikeClick;

			PART_Clear.MouseEnter += OnClearMouseEnter;
			PART_Clear.MouseDown += OnClearMouseClick;

			PART_RatingPanel.MouseEnter += OnRatingsMouseEnter;

			mDislikePartColor = UnratedColor;
			mClearPartColor = UnratedColor;

			if( IsInDesignMode ) {
				RatingCount = 5;
				Rating = 3;
			}
		}

		public static DependencyProperty RatingProperty =
			DependencyProperty.Register( "Rating", typeof( int ), typeof( RatingControl ), 
			new FrameworkPropertyMetadata( cRatingUnrated, FrameworkPropertyMetadataOptions.AffectsRender, OnRatingChanged, CoerceRatingCallback ));
		public int Rating {
			get { return (int)GetValue( RatingProperty ); }
			set { SetValue( RatingProperty, value ); }
		}

		private static object CoerceRatingCallback( DependencyObject element, object value ) {
			var	retValue = value;

			if(( value is int ) &&
			   ( element is RatingControl )) {
				var	newRating = (int)value;
				var control = element as RatingControl;

				if( newRating > control.RatingCount ) {
					newRating = control.RatingCount;
				}

				if( newRating < -1 ) {
					newRating = -1;
				}

				retValue = newRating;
			}

			return( retValue );
		}

		private static void OnRatingChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if(( depObj is RatingControl ) &&
			   ( args.NewValue is int )) {
				( depObj as RatingControl ).OnRatingChanged();
			}
		}

		public void OnRatingChanged() {
			foreach( var item in mRatingItems ) {
				item.IsRated = item.Rating <= Rating;
			}

			SetPartColors();
		}

		public static DependencyProperty UseAlternateProperty =
			DependencyProperty.Register( "UseAlternate", typeof( bool ), typeof( RatingControl ), 
			new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.AffectsRender, OnUseAlternateChanged ));
		public bool UseAlternate {
			get { return (bool)GetValue( UseAlternateProperty ); }
			set { SetValue( UseAlternateProperty, value ); }
		}

		private static void OnUseAlternateChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if(( depObj is RatingControl ) &&
			   ( args.NewValue is bool )) {
			   ( depObj as RatingControl ).OnUseAlternateChanged();
			}
		}

		public void OnUseAlternateChanged() {
			foreach( var item in mRatingItems ) {
				item.IsAlternate = UseAlternate;
			}

			SetPartColors();
		}

		public static DependencyProperty RatingCountProperty =
			DependencyProperty.Register( "RatingCount", typeof( int ), typeof( RatingControl ), 
			new FrameworkPropertyMetadata( cInitialRatingCount, OnRatingCountChanged ));
		public int RatingCount {
			get { return (int)GetValue( RatingCountProperty ); }
			set { SetValue( RatingCountProperty, value ); }
		}

		private static void OnRatingCountChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if(( depObj is RatingControl ) &&
			   ( args.NewValue is int )) {
				( depObj as RatingControl ).SetRatingCount((int)args.NewValue );
			}
		}

		public void SetRatingCount( int count ) {
			mRatingItems.Clear();

			for( int index = 0; index < count; index++ ) {
				mRatingItems.Add( new UiRatingItem( index + 1, OnRatingItemHover, OnRatingItemClick, UnratedColor ));
			}

			SetPartColors();
		}

		public ObservableCollection<UiRatingItem> RatingItems {
			get{ return( mRatingItems ); }
		}

		public static readonly DependencyProperty HoverColorProperty =
			DependencyProperty.Register( "HoverColor", typeof( Brush ), typeof( RatingControl ),
			new FrameworkPropertyMetadata( Brushes.LightBlue, FrameworkPropertyMetadataOptions.AffectsRender, OnHoverColorChanged ));

		public Brush HoverColor {
			get { return (Brush)GetValue( HoverColorProperty ); }
			set { SetValue( HoverColorProperty, value ); }
		}

		public static void OnHoverColorChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if( depObj is RatingControl ) {
				( depObj as RatingControl ).OnHoverColorChange();
			}
		}

		public void OnHoverColorChange() {
			SetPartColors();
		}

		public static readonly DependencyProperty RatedColorProperty =
			DependencyProperty.Register( "RatedColor", typeof( Brush ), typeof( RatingControl ),
			new FrameworkPropertyMetadata( Brushes.Green, FrameworkPropertyMetadataOptions.AffectsRender, OnRatedColorChange ));

		public Brush RatedColor {
			get { return (Brush)GetValue( RatedColorProperty ); }
			set { SetValue( RatedColorProperty, value ); }
		}

		public static void OnRatedColorChange( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if( depObj is RatingControl ) {
				( depObj as RatingControl ).OnRatedColorChange();
			}
		}

		public void OnRatedColorChange() {
			SetRatingColors();
		}

		public static readonly DependencyProperty UnratedColorProperty =
			DependencyProperty.Register( "UnratedColor", typeof( Brush ), typeof( RatingControl ),
			new FrameworkPropertyMetadata( Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnUnratedColorChange ));

		public Brush UnratedColor {
			get { return (Brush)GetValue( UnratedColorProperty ); }
			set { SetValue( UnratedColorProperty, value ); }
		}

		public static void OnUnratedColorChange( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if( depObj is RatingControl ) {
				( depObj as RatingControl ).OnUnratedColorChange();
			}
		}

		public void OnUnratedColorChange() {
			SetPartColors();
			SetRatingColors();
		}

		public static readonly DependencyProperty DislikeColorProperty =
			DependencyProperty.Register( "DislikeColor", typeof( Brush ), typeof( RatingControl ),
			new FrameworkPropertyMetadata( Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender, OnDislikeColorChanged ));

		public Brush DislikeColor {
			get { return (Brush)GetValue( DislikeColorProperty ); }
			set { SetValue( DislikeColorProperty, value ); }
		}

		public static void OnDislikeColorChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs args ) {
			if( depObj is RatingControl ) {
				( depObj as RatingControl ).OnDislikeColorChanged();
			}
		}

		public void OnDislikeColorChanged() {
			SetPartColors();
		}

		public Brush DislikePartColor {
			get{ return( mDislikePartColor ); }
			set{
				mDislikePartColor = value;

				RaisePropertyChanged( () => DislikePartColor );
			}
		}

		private void OnDislikeMouseEnter( object sender, System.Windows.Input.MouseEventArgs e ) {
			SetHoverPart( HoverPart.Dislike );
		}

		private void OnDislikeClick( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
			Rating = cRatingDislike;
		}

		public Brush ClearPartColor {
			get{ return( mClearPartColor ); }
			set{
				mClearPartColor = value;

				RaisePropertyChanged( () => ClearPartColor );
			}
		}

		private void OnClearMouseEnter( object sender, System.Windows.Input.MouseEventArgs e ) {
			SetHoverPart( HoverPart.Clear );
		}

		private void OnClearMouseClick( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
			Rating = cRatingUnrated;
		}

		private void OnRatingsMouseEnter( object sender, System.Windows.Input.MouseEventArgs e ) {
			SetHoverPart( HoverPart.Rating );
		}

		private void OnRootMouseLeave( object sender, System.Windows.Input.MouseEventArgs e ) {
			mHoverPart = HoverPart.None;

			SetPartColors();
		}

		private void OnRatingItemHover( int ratingItem ) {
			foreach( var item in mRatingItems ) {
				item.IsHighlighted = item.Rating <= ratingItem;
			}

			SetHoverPart( HoverPart.Rating );
			SetRatingColors();
		}

		private void OnRatingItemClick( int ratingItem ) {
			Rating = ratingItem;
		}

		private void SetHoverPart( HoverPart part ) {
			mHoverPart = part;

			SetPartColors();
		}

		private void SetPartColors() {
			if( Rating == cRatingDislike ) {
				DislikePartColor = DislikeColor;
			}
			else {
				DislikePartColor = mHoverPart == HoverPart.Dislike ? HoverColor : UnratedColor;
			}

			ClearPartColor = mHoverPart == HoverPart.Clear ? HoverColor : UnratedColor;

			SetRatingColors();
		}


		private void SetRatingColors() {
			if( mHoverPart != HoverPart.Rating ) {
				foreach( var item in mRatingItems ) {
					item.IsHighlighted = false;
				}
			}

			foreach( var item in mRatingItems ) {
				item.PartColor = item.IsRated ? RatedColor : item.IsHighlighted ? HoverColor : UnratedColor;
			}
		}

		private void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ));
		}

		private void RaisePropertyChanged( string propertyName ) {
			if( PropertyChanged != null ) {
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ));
			}
		}

		private static string PropertyName<T>( Expression<Func<T>> expression ) {
			var memberExpression = expression.Body as MemberExpression;

			if( memberExpression == null )
				throw new ArgumentException( "expression must be a property expression" );

			return memberExpression.Member.Name;
		}
	}

	public class UiRatingItem : INotifyPropertyChanged {
		public	int						Rating { get; private set; }
		private bool					mIsAlternate;
		private	bool					mIsHighlighted;
		private	bool					mIsRated;
		private bool					mIsHovering;
		private	Brush					mPartColor;
		private readonly Action<int>	mOnHover;
		private readonly Action<int>	mOnClick;

		public	event PropertyChangedEventHandler		PropertyChanged;

		public UiRatingItem( int rating, Action<int> onHover, Action<int> onClick, Brush partColor ) {
			Rating = rating;
			mOnHover = onHover;
			mOnClick = onClick;
			PartColor = partColor;
		}

		public Brush PartColor {
			get{ return( mPartColor ); }
			set {
				mPartColor = value;

				RaisePropertyChanged( () => PartColor );
			}
		}

		public bool IsHovering {
			get{ return( mIsHovering ); }
			set {
				mIsHovering = value;

				if(( mIsHovering ) &&
				   ( mOnHover != null )) {
					mOnHover( Rating );
				}
			}
		}

		public bool Clicked {
			get{ return( false ); }
			set {
				if(( value ) &&
				   ( mOnClick != null )) {
					mOnClick( Rating );
				}
			} 
		}

		public bool IsAlternate {
			get {  return( mIsAlternate ); }
			set {
				mIsAlternate = value;

				RaisePropertyChanged( () => IsActive );
				RaisePropertyChanged( () => IsAlternateActive );
			}
		}

		public bool IsRated {
			get{ return( mIsRated ); }
			set {
				mIsRated = value;

				RaisePropertyChanged( () => IsActive );
				RaisePropertyChanged( () => IsAlternateActive );
			}
		}

		public bool IsHighlighted {
			get{ return( mIsHighlighted ); }
			set {
				mIsHighlighted = value;

				RaisePropertyChanged( () => IsActive );
				RaisePropertyChanged( () => IsAlternateActive );
			}
		}

		public bool IsActive {
			get{ return(( IsRated || IsHighlighted ) && !mIsAlternate ); }
		}

		public bool IsAlternateActive {
			get { return(( IsRated || IsHighlighted ) && mIsAlternate ); }
		}

		private void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ));
		}

		private void RaisePropertyChanged( string propertyName ) {
			if( PropertyChanged != null ) {
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ));
			}
		}

		private static string PropertyName<T>( Expression<Func<T>> expression ) {
			var memberExpression = expression.Body as MemberExpression;

			if( memberExpression == null )
				throw new ArgumentException( "expression must be a property expression" );

			return memberExpression.Member.Name;
		}
	}

	public class BooleanToVisibleConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var	retValue = Visibility.Visible;

			if( targetType != typeof( Visibility ))
				throw new InvalidOperationException( "The target must be Visibility property" );

			if( value is bool ) {
				retValue = (bool)value ? Visibility.Visible : Visibility.Hidden;
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( null );
		}
	}

	public class BooleanToHiddenConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var	retValue = Visibility.Hidden;

			if( targetType != typeof( Visibility ))
				throw new InvalidOperationException( "The target must be Visibility property" );

			if( value is bool ) {
				retValue = (bool)value ? Visibility.Hidden : Visibility.Visible;
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( null );
		}
	}

	public class RatingItemObserver {
		public static bool GetObserve( FrameworkElement elem ) {
			return (bool)elem.GetValue( ObserveProperty );
		}

		public static void SetObserve(
		  FrameworkElement elem, bool value ) {
			elem.SetValue( ObserveProperty, value );
		}

		public static readonly DependencyProperty ObserveProperty =
		DependencyProperty.RegisterAttached( "Observe", typeof( bool ), typeof( RatingItemObserver ),
			new UIPropertyMetadata( false, OnObserveChanged ) );

		static void OnObserveChanged(
		  DependencyObject depObj, DependencyPropertyChangedEventArgs e ) {
			var elem = depObj as FrameworkElement;
			if( elem == null )
				return;

			if( e.NewValue is bool == false )
				return;

			if( (bool)e.NewValue ) {
				elem.MouseEnter += OnMouseEnter;
				elem.MouseLeave += OnMouseLeave;
				elem.MouseDown += OnMouseDown;
			}
			else {
				elem.MouseEnter -= OnMouseEnter;
				elem.MouseLeave -= OnMouseLeave;
				elem.MouseDown -= OnMouseDown;
			}
		}

		static void OnMouseEnter( object sender, RoutedEventArgs e ) {
			if( !ReferenceEquals( sender, e.OriginalSource ) )
				return;

			var elem = e.OriginalSource as FrameworkElement;
			if( elem != null ) {
				SetHover( elem, true );
			}
		}

		static void OnMouseLeave( object sender, RoutedEventArgs e ) {
			if( !ReferenceEquals( sender, e.OriginalSource ) )
				return;

			var elem = e.OriginalSource as FrameworkElement;
			if( elem != null ) {
				SetHover( elem, false );
			}
		}

		static void OnMouseDown( object sender, RoutedEventArgs e ) {
			if( !ReferenceEquals( sender, e.OriginalSource ) )
				return;

			var elem = e.OriginalSource as FrameworkElement;
			if( elem != null ) {
				SetClick( elem, true  );
			}
		}

		// Using a DependencyProperty as the backing store for ObservedWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HoverProperty =
			DependencyProperty.RegisterAttached( "Hover", typeof( bool ), typeof( RatingItemObserver ), new UIPropertyMetadata( false ));

		public static bool GetHover( DependencyObject obj ) {
			return((bool)obj.GetValue( HoverProperty ));
		}

		public static void SetHover( DependencyObject obj, bool value ) {
			obj.SetValue( HoverProperty, value );
		}

		// Using a DependencyProperty as the backing store for ObservedWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ClickProperty =
			DependencyProperty.RegisterAttached( "Click", typeof( bool ), typeof( RatingItemObserver ), new UIPropertyMetadata( false ));

		public static bool GetClick( DependencyObject obj ) {
			return((bool)obj.GetValue( ClickProperty ));
		}

		public static void SetClick( DependencyObject obj, bool value ) {
			obj.SetValue( ClickProperty, value );
		}
	}
}
