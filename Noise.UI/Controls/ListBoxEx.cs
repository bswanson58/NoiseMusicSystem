using System;
using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Controls {
	public class ListBoxEx : ListBox {
		public enum ItemStyles {
			NormalStyle,
			CheckBoxStyle,
			RadioBoxStyle
		}
		private ItemStyles mExtendedStyle;

		public ItemStyles ExtendedStyle {
			get { return mExtendedStyle; }
			set {
				mExtendedStyle = value;

				// load resources
				var resDict = new ResourceDictionary { Source = new Uri( "pack://application:,,,/Noise.UI;component/Resources/ListBoxEx.xaml" ) };
				if( resDict.Source == null ) {
					throw new SystemException();
				}

				switch( value ) {
					case ItemStyles.NormalStyle:
						Style = (Style)resDict["NormalListBox"];
						break;
					case ItemStyles.CheckBoxStyle:
						Style = (Style)resDict["CheckListBox"];
						break;
					case ItemStyles.RadioBoxStyle:
						Style = (Style)resDict["RadioListBox"];
						break;
				}
			}
		}

		static ListBoxEx() {
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ListBoxEx ), new FrameworkPropertyMetadata( typeof( ListBoxEx )));
		}

		public ListBoxEx( ItemStyles style ) {
			ExtendedStyle = style;
		}

		public ListBoxEx() {
			ExtendedStyle = ItemStyles.NormalStyle;
		}
	}
}
