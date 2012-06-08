using System.ComponentModel;
using FluentAssertions.EventMonitoring;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	internal class TestPropertyChangeBase : PropertyChangeBase {
		public int Property1 {
			get{ return( 1 ); }
		}

		public void RaiseTestEvent( string name ) {
			RaisePropertyChanged( name );
		}

		public void RaiseTestLambda() {
			RaisePropertyChanged( () => Property1 );
		}

		public void RaiseChangeAll() {
			RaiseAllPropertiesChanged();
		}
	}

	[TestFixture]
	public class PropertyChangeBaseTests {

		private TestPropertyChangeBase CreateSut() {
			return( new TestPropertyChangeBase());
		}

		[Test]
		public void CanRaisePropertyChanged() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.RaiseTestEvent( "property" );

			sut.ShouldRaise( "PropertyChanged" ).WithSender( sut ).WithArgs<PropertyChangedEventArgs>( args => args.PropertyName == "property" );
		}

		[Test]
		public void CanRaiseAllPropertiesChanged() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.RaiseChangeAll();

			sut.ShouldRaise( "PropertyChanged" ).WithArgs<PropertyChangedEventArgs>( args => args.PropertyName == string.Empty );
		}

		[Test]
		public void ShouldRaiseEventForLambda() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.RaiseTestLambda();

			sut.ShouldRaisePropertyChangeFor( subject => subject.Property1 );
		}
	}
}
