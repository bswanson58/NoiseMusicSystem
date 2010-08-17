using System;
using System.Windows;
using System.Windows.Input;

namespace Noise.UI.Behaviours {
	public static class EventBehaviourFactory {
		public static DependencyProperty CreateCommandExecutionEventBehaviour( RoutedEvent routedEvent, string propertyName, Type ownerType ) {
			DependencyProperty property = DependencyProperty.RegisterAttached( propertyName, typeof( ICommand ), ownerType,
															   new PropertyMetadata( null,
																   new ExecuteCommandOnRoutedEventBehaviour( routedEvent ).PropertyChangedHandler ) );
			return property;
		}

		/// <summary>
		/// An internal class to handle listening for an event and executing a command,
		/// when a Command is assigned to a particular DependencyProperty
		/// </summary>
		private class ExecuteCommandOnRoutedEventBehaviour : ExecuteCommandBehaviour {
			private readonly RoutedEvent mRoutedEvent;

			public ExecuteCommandOnRoutedEventBehaviour( RoutedEvent routedEvent ) {
				mRoutedEvent = routedEvent;
			}

			/// <summary>
			/// Handles attaching or Detaching Event handlers when a Command is assigned or unassigned
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="oldValue"></param>
			/// <param name="newValue"></param>
			protected override void AdjustEventHandlers( DependencyObject sender, object oldValue, object newValue ) {
				var element = sender as UIElement;
				if( element == null ) { return; }

				if( oldValue != null ) {
					element.RemoveHandler( mRoutedEvent, new RoutedEventHandler( EventHandler ) );
				}

				if( newValue != null ) {
					element.AddHandler( mRoutedEvent, new RoutedEventHandler( EventHandler ) );
				}
			}

			private void EventHandler( object sender, RoutedEventArgs e ) {
				HandleEvent( sender, e );
			}
		}

		internal abstract class ExecuteCommandBehaviour {
			private	DependencyProperty mProperty;
			protected abstract void AdjustEventHandlers( DependencyObject sender, object oldValue, object newValue );

			protected void HandleEvent( object sender, EventArgs e ) {
				var dp = sender as DependencyObject;
				if( dp == null ) {
					return;
				}

				var command = dp.GetValue( mProperty ) as ICommand;

				if( command == null ) {
					return;
				}

				if( command.CanExecute( e ) ) {
					command.Execute( e );
				}
			}

			/// <summary>
			/// Listens for a change in the DependencyProperty that we are assigned to, and
			/// adjusts the EventHandlers accordingly
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			public void PropertyChangedHandler( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
				// the first time the property changes,
				// save which property we are supposed
				// to be watching
				if( mProperty == null ) {
					mProperty = e.Property;
				}

				object oldValue = e.OldValue;
				object newValue = e.NewValue;

				AdjustEventHandlers( sender, oldValue, newValue );
			}
		}
	}
}
