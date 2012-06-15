using System.Windows.Input;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	[TestFixture]
	public class AutomaticCommandBaseTests {
		internal class AutomaticCommandProperties : AutomaticCommandBase {
			public bool SomethingWasExecuted { get; set; }
			public bool CanExecuteSomethingWasExecuted { get; set; }

			public void Execute_Something() {
				SomethingWasExecuted = true;
			}

			public bool CanExecuteResult { get; set; }

			[DependsUpon( "Text" )]
			public bool CanExecute_Something() {
				CanExecuteSomethingWasExecuted = true;
				return CanExecuteResult;
			}

			public string Text {
				get { return Get( () => Text ); }
				set { Set( () => Text, value ); }
			}
		}

		[Test]
		public void ExecuteMethodGeneratesICommandProperty() {
			dynamic sut = new AutomaticCommandProperties();

			var command = sut.Something;

			Assert.That( command is ICommand );
		}

		[Test]
		public void ExecuteMethodICommandExecuteCanBeCalled() {
			dynamic sut = new AutomaticCommandProperties();

			sut.Something.Execute( null );

			Assert.That( sut.SomethingWasExecuted );
		}

		[Test]
		public void WhenDependantPropertyChangesCanExecuteDoesNotExecute() {
			dynamic sut = new AutomaticCommandProperties();

			sut.Text = "Something";

			Assert.That( sut.CanExecuteSomethingWasExecuted, Is.False );
		}

		[TestCase( true )]
		[TestCase( false )]
		public void CanExecuteMethodIsWrappedByICommand( bool predicateResult ) {
			dynamic sut = new AutomaticCommandProperties();

			sut.CanExecuteResult = predicateResult;

			Assert.That( sut.Something.CanExecute( null ), Is.EqualTo( predicateResult ) );
		}

		[Test]
		public void ChangingPropertyCausesCanExecuteChangedToFire() {
			dynamic sut = new AutomaticCommandProperties();
			bool canExecuteChangedFired = false;

			var command = sut.Something as ICommand;

			Assert.IsNotNull( command );
			command.CanExecuteChanged += ( s, e ) => canExecuteChangedFired = true;

			sut.Text = "Foo";

			Assert.That( canExecuteChangedFired );
		}
	}
}
