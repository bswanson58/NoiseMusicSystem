using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ReusableBits.Ui.Behaviours {
	// usage:
	//
	//<ItemsControl ... >
    //<i:Interaction.Behaviors>
    //    <Behaviours:EmptyListBehavior
	//				EmptyTemplate="templateName"
	//				ProgressTemplate="templateName"
	//				IsUpdating="boolean value" />
    //</i:Interaction.Behaviors>
	//</ItemsControl>
	public class EmptyListBehavior : Behavior<ItemsControl> {
		private ControlTemplate	mDefaultTemplate;
		private ItemsControl	mItemsControl;
		private bool			mDefaultTemplateCollected;

		public static readonly DependencyProperty EmptyTemplateProperty = DependencyProperty.Register(
			"EmptyTemplate",
			typeof( ControlTemplate ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( null ) );

		public ControlTemplate EmptyTemplate {
			get { return GetValue( EmptyTemplateProperty ) as ControlTemplate; }
			set { SetValue( EmptyTemplateProperty, value ); }
		}

		public static readonly DependencyProperty ProgressTemplateProperty = DependencyProperty.Register(
			"ProgressTemplate",
			typeof( ControlTemplate ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( null ) );

		public ControlTemplate ProgressTemplate {
			get { return GetValue( ProgressTemplateProperty ) as ControlTemplate; }
			set { SetValue( ProgressTemplateProperty, value ); }
		}

		public static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.Register(
			"IsUpdating",
			typeof( bool ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( false, IsUpdatingChanged ));

		public bool IsUpdating {
			get { return (bool)GetValue( IsUpdatingProperty ); }
			set { SetValue( IsUpdatingProperty, value ); }
		}

		private static void IsUpdatingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			var behavior = d as EmptyListBehavior;

			if(( behavior != null ) &&
			   ( behavior.mItemsControl != null )) {
				behavior.SetTemplate();
			}
		}

		private void SetTemplate() {
			if( mDefaultTemplateCollected ) {
				if(( IsUpdating ) &&
				   ( ProgressTemplate != null )) {
					mItemsControl.Template = ProgressTemplate;
				}
				else {
					if((!mItemsControl.HasItems ) &&
					   ( EmptyTemplate != null )) {
						mItemsControl.Template = EmptyTemplate;
					}
					else {
						if( mDefaultTemplate != null ) {
							mItemsControl.Template = mDefaultTemplate;
						}
					}
				}
			}
		}

		private void OnLoaded( object sender, RoutedEventArgs args ) {
			mItemsControl.Loaded -= OnLoaded;

			mDefaultTemplate = mItemsControl.Template;
			mDefaultTemplateCollected = true;

			SetTemplate();
		}

		private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
			SetTemplate();
		}

		protected override void OnAttached() {
			base.OnAttached();

			mItemsControl = AssociatedObject;

			if( mItemsControl != null ) {
				mItemsControl = AssociatedObject;

				mItemsControl.Loaded += OnLoaded;
				if( mItemsControl.Items != null ) {
					((INotifyCollectionChanged)( mItemsControl.Items )).CollectionChanged += OnCollectionChanged;
				}
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if( mItemsControl != null ) {
				mItemsControl.Loaded -= OnLoaded;

				if( mItemsControl.Items != null ) {
					((INotifyCollectionChanged)( mItemsControl.Items )).CollectionChanged -= OnCollectionChanged;
				}
			}
		}
	}
}