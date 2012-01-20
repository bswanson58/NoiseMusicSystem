using System;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Noise.Desktop;
using Noise.UI.Support;
using Noise.UI.Tests.MockingEventAggregator;
using Noise.UI.Tests.Support;
using Noise.UI.ViewModels;
using Noise.UI.Views;

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
			var container = new Mock<IUnityContainer>();
			container.Setup( m => m.Resolve( It.Is<Type>( p => p == typeof( ArtistViewModel )), It.IsAny<string>(), 
											 It.IsAny<ResolverOverride[]>())).Returns( CreateViewModel());
			ViewModelResolver.Container = container.Object;
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
