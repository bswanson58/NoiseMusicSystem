using NUnit.Framework;
using Noise.Desktop;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using Noise.UI.Views;
using ReusableBits.TestSupport.MockingEventAggregator;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.Views {
	[TestFixture]
	public class ArtistViewTests {

		private ArtistViewModel CreateViewModel() {
			return( new ArtistViewModel( new AutoMockingEventAggregator(), null, null, null, null, null ));
		}

		private void AssertMessage( string message ) {
			Assert.Fail( message );
		}

		[Test]
		[Ignore( "This inly works in when running under the debugger." )]
		public void AreBindingsValid() {
			ViewModelResolver.TypeResolver = ( type => CreateViewModel());
			BindingErrorListener.Listen( AssertMessage );

			var runner = new CrossThreadTestRunner();
			runner.RunInSTA( delegate {
				var app = new App();
				app.InitializeComponent();

				new ArtistView();

				app.Shutdown();
			});
		}
	}
}
