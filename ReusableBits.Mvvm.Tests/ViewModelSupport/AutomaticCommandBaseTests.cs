using System.Windows.Input;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	[TestFixture]
	public class AutomaticCommandBaseTests {
		internal class TestAutomaticCommandBase : AutomaticCommandBase {
			public bool	CommandExecuted { get; private set; }

			public void Execute_Command() {
				CommandExecuted = true;	
			}

			public ICommand RetrieveCommand( string name ) {
				return( Get<ICommand>( name ));
			}
		}

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
		public void Execute_Method_Generates_ICommand_Property() {
			dynamic viewModel = new AutomaticCommandProperties();

			var command = viewModel.Something;

			Assert.That( command is ICommand );
		}

		[Test]
		public void Execute_Method_Is_Wrapped_By_Dynamic_Property() {
			dynamic viewModel = new AutomaticCommandProperties();

			viewModel.Something.Execute( null );

			Assert.That( viewModel.SomethingWasExecuted );
		}

		[Test]
		public void When_Dependant_Changes_On_CanExecute_It_Does_Not_Execute() {
			dynamic viewModel = new AutomaticCommandProperties();

			viewModel.Text = "Something";

			Assert.That( viewModel.CanExecuteSomethingWasExecuted, Is.False );
		}

		[TestCase( true )]
		[TestCase( false )]
		public void CanExecute_Method_Is_Wrapped_By_Dynamic_Property( bool predicateResult ) {
			dynamic viewModel = new AutomaticCommandProperties();

			viewModel.CanExecuteResult = predicateResult;

			Assert.That( viewModel.Something.CanExecute( null ), Is.EqualTo( predicateResult ) );
		}

		[Test]
		public void Changing_Text_Causes_CanExecuteChanged_To_Fire() {
			dynamic viewModel = new AutomaticCommandProperties();
			bool canExecuteChangedFired = false;

			var command = viewModel.Something as ICommand;

			Assert.IsNotNull( command );
			command.CanExecuteChanged += ( s, e ) => canExecuteChangedFired = true;

			viewModel.Text = "Foo";

			Assert.That( canExecuteChangedFired );
		}
	}
}
