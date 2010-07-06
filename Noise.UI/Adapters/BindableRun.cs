using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace Noise.UI.Adapters {
	public class BindableRun : Run {
		public static readonly DependencyProperty BoundTextProperty = DependencyProperty.Register( "BoundText", typeof( string ), typeof( BindableRun ),
			new PropertyMetadata( new PropertyChangedCallback( BindableRun.OnBoundTextChanged ) ) );

		private static void OnBoundTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			( (Run)d ).Text = (string)e.NewValue;
		}

		public BindableRun()
			: base() {
			var b = new Binding( "DataContext" ) { RelativeSource = new RelativeSource( RelativeSourceMode.FindAncestor, typeof( FrameworkElement ), 1 ) };

			SetBinding( DataContextProperty, b );
		}
		public String BoundText {
			get { return (string)GetValue( BoundTextProperty ); }
			set { SetValue( BoundTextProperty, value ); }
		}
	}
}
