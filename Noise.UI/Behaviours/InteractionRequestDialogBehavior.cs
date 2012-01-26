using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.UI.Support;

namespace Noise.UI.Behaviours {
	public class InteractionRequestDialogBehavior : Behavior<FrameworkElement> {
		public IInteractionRequest InteractionRequest {
			get { return (IInteractionRequest)GetValue( InteractionRequestProperty ); }
			set { SetValue( InteractionRequestProperty, value ); }
		}

		// Using a DependencyProperty as the backing store for InteractionRequest.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InteractionRequestProperty =
            DependencyProperty.Register( "InteractionRequest", typeof( IInteractionRequest ),
										typeof( InteractionRequestDialogBehavior ), new UIPropertyMetadata( null ));

		// The class name of the dialog view to be used.
		public string DialogClass {
			get{ return( (string)GetValue( DialogClassProperty )); }
			set{ SetValue( DialogClassProperty, value ); }
		}
		public static readonly DependencyProperty DialogClassProperty =
			DependencyProperty.Register( "DialogClass", typeof( string ), typeof( InteractionRequestDialogBehavior ));

		// The service used to 
		public static readonly DependencyProperty DialogServiceProperty =
			DependencyProperty.Register( "DialogService", typeof( IDialogService ), typeof( InteractionRequestDialogBehavior ));

		public IDialogService DialogService {
			get{ return( (IDialogService)GetValue( DialogServiceProperty )); }
			set{ SetValue( DialogServiceProperty, value ); }
		}

		protected override void OnAttached() {
			AssociatedObject.Loaded += AssociatedObjectLoaded;
		}

		void AssociatedObjectLoaded( object sender, RoutedEventArgs e ) {
			InteractionRequest.Raised += InteractionRequestRaised;
		}

		void InteractionRequestRaised( object sender, InteractionRequestedEventArgs e ) {
			if( e.Context is Confirmation ) {
				var confirmation = e.Context as Confirmation;
				var dialogService = DialogService;

				if( dialogService != null ) {
					confirmation.Confirmed = dialogService.ShowDialog( DialogClass, e.Context.Content ) == true;
				}

				e.Callback.Invoke();
			}
		}

		protected override void OnDetaching() {
			AssociatedObject.Loaded -= AssociatedObjectLoaded;
			InteractionRequest.Raised -= InteractionRequestRaised;
		}
	}
}
