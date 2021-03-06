﻿using System;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ReusableBits.Ui.Behaviours {
	//<ListBox ... >
	//<i:Interaction.Behaviors>
	//    <Behaviours:ListBoxSelectionVisible />
	//</i:Interaction.Behaviors>
	//</ListBox>
	public class ListBoxSelectionVisible : Behavior<ListBox> {
		/// <summary>
		///  When Beahvior is attached
		/// </summary>
		protected override void OnAttached() {
			base.OnAttached();
			AssociatedObject.SelectionChanged += OnSelectionChanged;
		}

		/// <summary>
		/// When behavior is detached
		/// </summary>
		protected override void OnDetaching() {
			base.OnDetaching();
			AssociatedObject.SelectionChanged -= OnSelectionChanged;
		}

		private void OnSelectionChanged( object sender, SelectionChangedEventArgs e ) {
			if( sender is ListBox ) {
				var listBox = ( sender as ListBox );

				if( listBox.SelectedItem != null ) {
					listBox.Dispatcher.BeginInvoke((Action)( () => {
							listBox.UpdateLayout();

							if( listBox.SelectedItem != null ) {
								listBox.ScrollIntoView( listBox.SelectedItem );
							}
						} ));
				}
			}
		}
	}
}