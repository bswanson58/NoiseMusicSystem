namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for SmallPlayerView.xaml
	/// </summary>
	public partial class SmallPlayerView {
		public SmallPlayerView() {
			InitializeComponent();
		}

		protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e ) {
			DragMove();
		}
	}
}
