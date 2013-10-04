using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for Shell.xaml
	/// </summary>
	public partial class Shell {
		public Shell() {
			InitializeComponent();
		}

		public override void OnApplyTemplate() {
			var titleBar = GetTemplateChild( "PART_TitleBar" );

			// Bit of a nasty hack to give the application icon some breathing room in the title bar.
			if( titleBar is Panel ) {
				var panel = titleBar as Panel;
				var iconImage = ( from FrameworkElement control in panel.Children where control is Image select control ).FirstOrDefault();

				if( iconImage != null ) {
					iconImage.Margin = new Thickness( 5, 5, -3, 5 );
				}
			}

			base.OnApplyTemplate();
		}
	}
}
