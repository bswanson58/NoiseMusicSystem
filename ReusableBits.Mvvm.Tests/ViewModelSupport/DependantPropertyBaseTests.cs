using System;
using FluentAssertions.EventMonitoring;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	[TestFixture]
	public class DependantPropertyBaseTests {
		internal class TestPropertyChangeBase : DependantPropertyBase {
			private int mProperty4 = 4;

			public int Property4 {
				get { return( mProperty4 ); }
				set {
					mProperty4 = value;
					RaisePropertyChanged( () => Property4 );
				}
			}

			public int Property1 {
				get { return ( 1 ); }
			}

			[DependsUpon( "Property1" )]
			public int Property2 {
				get { return ( 2 ); }
			}

			[DependsUpon( "Property2" )]
			[DependsUpon( "Property4" )]
			public int Property3 {
				get { return ( 3 ); }
			}

			public void RaiseTestEvent( string name ) {
				RaisePropertyChanged( name );
			}

			public void RaiseTestLambda() {
				RaisePropertyChanged( () => Property1 );
			}

			public void SetProperty4() {
				Property4 = 5;
			}
		}

		internal class BadPropertyDependancy : DependantPropertyBase {
			[DependsUpon( "missingpropertyname" )]
			public int BadProperty {
				get { return ( 0 ); }
			}
		}

		private TestPropertyChangeBase CreateSut() {
			return ( new TestPropertyChangeBase() );
		}

		[Test]
		public void CanRaiseDependentProperty() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.RaiseTestEvent( "Property1" );

			sut.ShouldRaisePropertyChangeFor( p => p.Property2 );
		}

		[Test]
		public void ShouldCascadeToDependentProperty() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.RaiseTestEvent( "Property1" );

			sut.ShouldRaisePropertyChangeFor( subject => subject.Property3 );
		}

		[Test]
		public void MultipleDependantPropertiesFiresChange() {
			var sut = CreateSut();
			sut.MonitorEvents();

			sut.SetProperty4();

			sut.ShouldRaisePropertyChangeFor( subject => subject.Property3 );
		}

		[Test]
		[ExpectedException( typeof( ArgumentException ) )]
		public void ShouldSignalOnBadDependancyProperty() {
			new BadPropertyDependancy();
		}
	}
}
