using System;
using System.Windows;
using System.Windows.Controls;

// from: http://tdanemar.wordpress.com/2009/11/15/using-the-visualstatemanager-with-the-model-view-viewmodel-pattern-in-wpf-or-silverlight/
// Add to root of control: Behaviours:VisualStateAdapter.VisualState="{Binding VisualStateName}"

namespace Noise.UI.Behaviours {
	public class VisualStateAdapter : DependencyObject {
		public static string GetVisualState( DependencyObject obj ) {
			return (string)obj.GetValue( VisualStateProperty );
		}

		public static void SetVisualState( DependencyObject obj, string value ) {
			obj.SetValue( VisualStateProperty, value );
		}

		public static readonly DependencyProperty VisualStateProperty =
		DependencyProperty.RegisterAttached( "VisualState", typeof( string ), typeof( VisualStateAdapter ),
			new PropertyMetadata( ( ctrl, e ) => {
				if( ctrl is Control ) {
					VisualStateManager.GoToState( ctrl as Control, (string)e.NewValue, true );
				}
				else {
					throw new InvalidOperationException( "This attached property only supports types derived from Control." );
				}
			} ));
	}
}
