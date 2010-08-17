using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Noise.UI.Behaviours;

namespace Noise.UI.Controls {
	public class SearchEventArgs : RoutedEventArgs {
		public string Keyword { get; set; }
		public List<string> Sections { get; set; }

		public SearchEventArgs() {
			Sections = new List<string>();
		}

		public SearchEventArgs( RoutedEvent routedEvent )
			: base( routedEvent ) {
			Sections = new List<string>();
		}
	}

	public static class SearchTextBoxBehaviour {
		public static readonly DependencyProperty SearchCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour( SearchTextBox.SearchEvent, "SearchCommand", typeof( SearchTextBoxBehaviour ));

		public static void SetSearchCommand( DependencyObject o, ICommand value ) {
			o.SetValue( SearchCommand, value );
		}

		public static ICommand GetSearchCommand( DependencyObject o ) {
			return o.GetValue( SearchCommand ) as ICommand;
		}
	}

	public class SearchTextBox : TextBox {
		private readonly Popup	mListPopup = new Popup();
		private ListBoxEx		mListSection;
		private ListBoxEx		mListPreviousItem;

		public static DependencyProperty LabelTextProperty =
			DependencyProperty.Register(
				"LabelText",
				typeof( string ),
				typeof( SearchTextBox ));

		public static DependencyProperty LabelTextColorProperty =
			DependencyProperty.Register(
				"LabelTextColor",
				typeof( Brush ),
				typeof( SearchTextBox ));

		private readonly static DependencyPropertyKey HasTextPropertyKey =
			DependencyProperty.RegisterReadOnly(
				"HasText",
				typeof( bool ),
				typeof( SearchTextBox ),
				new PropertyMetadata());
		public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

		private readonly static DependencyPropertyKey IsMouseLeftButtonDownPropertyKey =
			DependencyProperty.RegisterReadOnly(
				"IsMouseLeftButtonDown",
				typeof( bool ),
				typeof( SearchTextBox ),
				new PropertyMetadata());
		public static DependencyProperty IsMouseLeftButtonDownProperty = IsMouseLeftButtonDownPropertyKey.DependencyProperty;

		public static readonly RoutedEvent SearchEvent = 
			EventManager.RegisterRoutedEvent(
				"Search",
				RoutingStrategy.Bubble,
				typeof( RoutedEventHandler ),
				typeof( SearchTextBox ));

		static SearchTextBox() {
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof( SearchTextBox ),
				new FrameworkPropertyMetadata( typeof( SearchTextBox )));
		}

		protected override void OnTextChanged( TextChangedEventArgs e ) {
			base.OnTextChanged( e );

			HasText = Text.Length != 0;
			HidePopup();
		}

		protected override void OnMouseDown( MouseButtonEventArgs e ) {
			base.OnMouseDown( e );

			// if users click on the editing area, the pop up will be hidden
			Type sourceType=e.OriginalSource.GetType();

			if( sourceType != typeof( Image )) {
				HidePopup();
			}
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			MouseLeave += SearchTextBox_MouseLeave;
			var iconBorder = GetTemplateChild( "PART_SearchIconBorder" ) as Border;
			if( iconBorder != null ) {
				iconBorder.MouseLeftButtonDown += IconBorder_MouseLeftButtonDown;
				iconBorder.MouseLeftButtonUp += IconBorder_MouseLeftButtonUp;
				iconBorder.MouseLeave += IconBorder_MouseLeave;
				iconBorder.MouseDown += SearchIcon_MouseDown;
			}

			int size = 0;
			if( ShowSectionButton ) {
				iconBorder = GetTemplateChild( "PART_SpecifySearchType" ) as Border;
				if( iconBorder != null ) {
					iconBorder.MouseDown += ChooseSection_MouseDown;
				}
				size = 15;
			}

			var iconChoose = GetTemplateChild( "SpecifySearchType" ) as Image;
			if( iconChoose != null ) {
				iconChoose.Width = iconChoose.Height = size;
			}

			iconBorder = GetTemplateChild( "PART_PreviousItem" ) as Border;
			if( iconBorder != null ) {
				iconBorder.MouseDown += PreviousItem_MouseDown;
			}
		}

		void SearchIcon_MouseDown( object sender, MouseButtonEventArgs e ) {
			HidePopup();
		}

		void SearchTextBox_MouseLeave( object sender, MouseEventArgs e ) {
			if( !mListPopup.IsMouseOver ) {
				HidePopup();
			}
		}

		private void IconBorder_MouseLeftButtonDown( object obj, MouseButtonEventArgs e ) {
			IsMouseLeftButtonDown = true;
		}

		private void IconBorder_MouseLeftButtonUp( object obj, MouseButtonEventArgs e ) {
			if( !IsMouseLeftButtonDown ) return;

			if( HasText ) {
				RaiseSearchEvent();
			}

			IsMouseLeftButtonDown = false;
		}

		private void IconBorder_MouseLeave( object obj, MouseEventArgs e ) {
			IsMouseLeftButtonDown = false;

		}

		protected override void OnKeyDown( KeyEventArgs e ) {
			if( e.Key == Key.Escape ) {
				Text = "";
			}
			else if( ( e.Key == Key.Return || e.Key == Key.Enter )) {
				RaiseSearchEvent();
			}
			else {
				base.OnKeyDown( e );
			}
		}

		private void RaiseSearchEvent() {
			if( Text == "" ) {
				return;
			}
			if( !mListPreviousItem.Items.Contains( Text )) {
				mListPreviousItem.Items.Add( Text );
			}


			var args = new SearchEventArgs( SearchEvent ) { Keyword = Text };
			if( mListSection != null ) {
				args.Sections = mListSection.SelectedItems.Cast<string>().ToList();
			}
			RaiseEvent( args );
		}

		public string LabelText {
			get { return (string)GetValue( LabelTextProperty ); }
			set { SetValue( LabelTextProperty, value ); }
		}

		public Brush LabelTextColor {
			get { return (Brush)GetValue( LabelTextColorProperty ); }
			set { SetValue( LabelTextColorProperty, value ); }
		}

		public bool HasText {
			get { return (bool)GetValue( HasTextProperty ); }
			private set { SetValue( HasTextPropertyKey, value ); }
		}

		public bool IsMouseLeftButtonDown {
			get { return (bool)GetValue( IsMouseLeftButtonDownProperty ); }
			private set { SetValue( IsMouseLeftButtonDownPropertyKey, value ); }
		}

		public event RoutedEventHandler OnSearch {
			add { AddHandler( SearchEvent, value ); }
			remove { RemoveHandler( SearchEvent, value ); }
		}

		public static DependencyProperty SectionsListProperty =
			DependencyProperty.Register(
				"SectionsList",
				typeof( List<string> ),
				typeof( SearchTextBox ),
				new FrameworkPropertyMetadata( null,
												FrameworkPropertyMetadataOptions.None )
			 );

		public List<string> SectionsList {
			get { return (List<string>)GetValue( SectionsListProperty ); }
			set {
				SetValue( SectionsListProperty, value );

			}
		}

		private bool mShowSectionButton = true;

		public bool ShowSectionButton {
			get { return mShowSectionButton; }
			set { mShowSectionButton = value; }
		}

		public enum SectionsStyles {
			NormalStyle,
			CheckBoxStyle,
			RadioBoxStyle
		}
		private SectionsStyles mItemStyle = SectionsStyles.CheckBoxStyle;

		public SectionsStyles SectionsStyle {
			get { return mItemStyle; }
			set { mItemStyle = value; }
		}

		private void BuildPopup() {
			// initialize the pop up
			mListPopup.PopupAnimation = PopupAnimation.Fade;
			mListPopup.Placement = PlacementMode.Relative;
			mListPopup.PlacementTarget = this;
			mListPopup.PlacementRectangle = new Rect( 0, ActualHeight, 30, 30 );
			mListPopup.Width = ActualWidth;
			// initialize the sections' list
			if( ShowSectionButton ) {
				mListSection = new ListBoxEx( (int)mItemStyle + ListBoxEx.ItemStyles.NormalStyle );

				mListSection.Items.Clear();
				if( SectionsList != null ) {
					foreach( string item in SectionsList ) {
						mListSection.Items.Add( item );
					}
				}

				mListSection.Width = Width;
				mListSection.MouseLeave += ListSection_MouseLeave;

			}

			// initialize the previous items' list
			mListPreviousItem = new ListBoxEx();
			mListPreviousItem.MouseLeave += ListPreviousItem_MouseLeave;
			mListPreviousItem.SelectionChanged += ListPreviousItem_SelectionChanged;
			mListPreviousItem.Width = Width;
		}

		private void HidePopup() {
			mListPopup.IsOpen = false;
		}

		private void ShowPopup( UIElement item ) {
			mListPopup.StaysOpen = true;

			mListPopup.Child = item;
			mListPopup.IsOpen = true;

		}

		void ListPreviousItem_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
			// fetch the previous keyword into the text box
			Text = mListPreviousItem.SelectedItems[0].ToString();
			// close the pop up
			HidePopup();

			Focus();
			SelectionStart = Text.Length;
		}

		void ListPreviousItem_MouseLeave( object sender, MouseEventArgs e ) {
			// close the pop up
			HidePopup();
		}

		void PreviousItem_MouseDown( object sender, MouseButtonEventArgs e ) {
			if( mListPreviousItem.Items.Count != 0 )
				ShowPopup( mListPreviousItem );
		}

		void ListSection_MouseLeave( object sender, MouseEventArgs e ) {
			// close the pop up
			HidePopup();
		}

		void ChooseSection_MouseDown( object sender, MouseButtonEventArgs e ) {

			if( SectionsList == null )
				return;
			if( SectionsList.Count != 0 )
				ShowPopup( mListSection );
		}

		protected override void OnLostFocus( RoutedEventArgs e ) {
			base.OnLostFocus( e );
			if( !HasText )
				LabelText = "Search";

			mListPopup.StaysOpen = false;
		}

		protected override void OnGotFocus( RoutedEventArgs e ) {
			base.OnGotFocus( e );
			if( !HasText )
				LabelText = "";
			mListPopup.StaysOpen = true;

		}

		protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo ) {
			base.OnRenderSizeChanged( sizeInfo );
			BuildPopup();
		}
	}
}
