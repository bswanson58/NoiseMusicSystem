using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	[TestFixture]
	public class AutomaticPropertyBaseTests {
		private class GetterSetterByString : AutomaticPropertyBase {
			public string Foo {
				get { return Get<string>( "Foo" ); }
				set { Set( "Foo", value ); }
			}

			public int MyInt {
				get { return Get<int>( "MyInt" ); }
				set { Set( "MyInt", value ); }
			}

			public int IntWithDefault {
				get { return Get( "IntWithDefault", 56 ); }
				set { Set( "IntWithDefault", value ); }
			}
		}

		[Test]
		public void ValuseSetByStringCanBeRetrieved() {
			var viewModel = new GetterSetterByString { Foo = "Bar" };

			Assert.That( viewModel.Foo, Is.EqualTo( "Bar" ) );
		}

		[Test]
		public void Int_Values_Can_Be_Gotten_And_Set() {
			var viewModel = new GetterSetterByString { MyInt = 55 };

			Assert.That( viewModel.MyInt, Is.EqualTo( 55 ) );
		}

		[Test]
		public void Setting_Value_Sends_PropertyChanged_Event() {
			var viewModel = new GetterSetterByString();
			string changedProperties = string.Empty;
			viewModel.PropertyChanged += ( s, e ) => changedProperties += e.PropertyName;

			viewModel.Foo = "Bar";

			Assert.That( changedProperties, Is.EqualTo( "Foo" ) );
		}

		[Test]
		public void Setting_Twice_Does_Not_Fail() {
			var viewModel = new GetterSetterByString { Foo = "Bar" };

			viewModel.Foo = "Baz";

			Assert.That( viewModel.Foo, Is.EqualTo( "Baz" ) );
		}

		[Test]
		public void Setting_To_Same_Value_Does_Not_Fire_PropertyChanged_Twice() {
			var viewModel = new GetterSetterByString();
			string changedProperties = string.Empty;
			viewModel.PropertyChanged += ( s, e ) => changedProperties += e.PropertyName;

			viewModel.Foo = "Bar";
			viewModel.Foo = "Bar";

			Assert.That( changedProperties, Is.EqualTo( "Foo" ) );
		}

		[Test]
		public void Setting_To_Same_Null_Value_Does_Not_Fire_PropertyChanged_Twice() {
			var viewModel = new GetterSetterByString();
			string changedProperties = string.Empty;
			viewModel.PropertyChanged += ( s, e ) => changedProperties += e.PropertyName;

			viewModel.Foo = "test";
			viewModel.Foo = null;
			viewModel.Foo = null;

			Assert.That( changedProperties, Is.EqualTo( "FooFoo" ) );
		}

		private class ViewModelWithMismatchedGetAndSetNames : AutomaticPropertyBase {
			public int Value {
				get { return Get<int>( "WrongName" ); }
				set { Set( "Value", value ); }
			}
		}

		[Test]
		public void Mismatched_Names_Returns_Default() {
			var viewModel = new ViewModelWithMismatchedGetAndSetNames { Value = 55 };

			Assert.That( viewModel.Value, Is.EqualTo( 0 ) );
		}

		[Test]
		public void Default_Values_On_Getter() {
			var viewModel = new GetterSetterByString();

			Assert.That( viewModel.IntWithDefault, Is.EqualTo( 56 ) );
		}

		[Test]
		public void Dynamic_Values_On_Setter() {
			var viewModel = new GetterSetterByString();

			( viewModel as dynamic ).MyDynamicProperty = "Me";

			Assert.That( ( viewModel as dynamic ).MyDynamicProperty, Is.EqualTo( "Me" ) );
		}

		[Test]
		public void Setting_With_A_Default_Getter_Retrieves_Set_Value() {
			var viewModel = new GetterSetterByString { IntWithDefault = 99 };

			Assert.That( viewModel.IntWithDefault, Is.EqualTo( 99 ) );
		}

		public class ViewModelWithSemantics : AutomaticPropertyBase {
			public string PropertyWithSemantics {
				get { return Get( () => PropertyWithSemantics ); }
				set { Set( () => PropertyWithSemantics, value ); }
			}

			public int PropertyWithSemanticsWithDefault {
				get { return Get( () => PropertyWithSemanticsWithDefault, 5 ); }
				set { Set( () => PropertyWithSemanticsWithDefault, value ); }
			}
		}

		[Test]
		public void When_Symantics_Are_Used_We_Can_Get_And_Set() {
			var viewModel = new ViewModelWithSemantics { PropertyWithSemantics = "Semantic" };

			Assert.That( viewModel.PropertyWithSemantics, Is.EqualTo( "Semantic" ) );
		}

		[Test]
		public void When_Symantics_Are_Used_We_Can_Get_With_Default_Value() {
			var viewModel = new ViewModelWithSemantics();

			Assert.That( viewModel.PropertyWithSemanticsWithDefault, Is.EqualTo( 5 ) );
		}

		private class InitialValueOnProperties : AutomaticPropertyBase {
			public int InitialValueCount;

			private string InitialValue() {
				InitialValueCount++;
				return "Default";
			}

			public string TestProperty {
				get { return Get( () => TestProperty, InitialValue ); }
			}

			public string TestStringProperty {
				get { return Get( "TestStringProperty", InitialValue ); }
			}
		}

		[Test]
		public void Initial_Value_Is_Set_On_String_Getter() {
			var viewModel = new InitialValueOnProperties();

			var value = viewModel.TestStringProperty;

			Assert.That( value, Is.EqualTo( "Default" ) );
		}

		[Test]
		public void Initial_Value_Is_Set_On_Lambda_Getter() {
			var viewModel = new InitialValueOnProperties();

			var value = viewModel.TestProperty;

			Assert.That( value, Is.EqualTo( "Default" ) );
		}

		[Test]
		public void Initial_Value_Is_Only_Requested_Once() {
			var viewModel = new InitialValueOnProperties();

			Assert.That( viewModel.InitialValueCount, Is.EqualTo( 0 ));
			var value = viewModel.TestProperty;

			Assert.That( viewModel.InitialValueCount, Is.EqualTo( 1 ));
			value = viewModel.TestProperty;

			Assert.That( viewModel.InitialValueCount, Is.EqualTo( 1 ));
		}

		internal class SingleDependency : AutomaticPropertyBase {
			public int InputA {
				get { return Get( () => InputA ); }
				set { Set( () => InputA, value ); }
			}

			[DependsUpon( "InputA" )]
			public int InputASquared {
				get { return InputA * InputA; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void When_InputA_Changes_Dependent_Notification_Fires() {
			var viewModel = new SingleDependency();
			var propertiesChanged = new List<string>();
			viewModel.PropertyChanged += ( s, e ) => propertiesChanged.Add( e.PropertyName );

			viewModel.InputA = 5;

			Assert.That( propertiesChanged.Count, Is.EqualTo( 2 ) );
			Assert.That( propertiesChanged[0], Is.EqualTo( "InputA" ) );
			Assert.That( propertiesChanged[1], Is.EqualTo( "InputASquared" ) );
		}

		internal class SinglePropertyMultipleDependencies : AutomaticPropertyBase {
			public int InputA {
				get { return Get( () => InputA ); }
				set { Set( () => InputA, value ); }
			}

			public int InputB {
				get { return Get( () => InputB ); }
				set { Set( () => InputB, value ); }
			}

			[DependsUpon( "InputA" )]
			[DependsUpon( "InputB" )]
			public int APlusB {
				get { return InputA + InputB; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void When_InputA_And_InputB_Changes_Dependent_Notification_Fires() {
			var viewModel = new SinglePropertyMultipleDependencies();
			var propertiesChanged = new List<string>();
			viewModel.PropertyChanged += ( s, e ) => propertiesChanged.Add( e.PropertyName );

			viewModel.InputA = 5;
			viewModel.InputB = 6;

			Assert.That( propertiesChanged.Count, Is.EqualTo( 4 ) );
			Assert.That( propertiesChanged[0], Is.EqualTo( "InputA" ) );
			Assert.That( propertiesChanged[1], Is.EqualTo( "APlusB" ) );
			Assert.That( propertiesChanged[2], Is.EqualTo( "InputB" ) );
			Assert.That( propertiesChanged[3], Is.EqualTo( "APlusB" ) );
		}

		internal class MultiplePropertiesSingleDependency : AutomaticPropertyBase {
			public int InputA {
				get { return Get( () => InputA ); }
				set { Set( () => InputA, value ); }
			}

			[DependsUpon( "InputA" )]
			public int InputASquared {
				get { return InputA * InputA; }
			}

			[DependsUpon( "InputA" )]
			public int InputACubed {
				get { return InputA * InputA * InputA; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void When_InputA_Changes_All_Dependent_Notifications_Fires() {
			var viewModel = new MultiplePropertiesSingleDependency();
			var propertiesChanged = new List<string>();
			viewModel.PropertyChanged += ( s, e ) => propertiesChanged.Add( e.PropertyName );

			viewModel.InputA = 5;

			Assert.That( propertiesChanged.Count, Is.EqualTo( 3 ) );
			Assert.That( propertiesChanged[0], Is.EqualTo( "InputA" ) );
			Assert.That( propertiesChanged[1], Is.EqualTo( "InputASquared" ) );
			Assert.That( propertiesChanged[2], Is.EqualTo( "InputACubed" ) );
		}

		internal class ChainedDependencies : AutomaticPropertyBase {
			public int InputA {
				get { return Get( () => InputA ); }
				set { Set( () => InputA, value ); }
			}

			[DependsUpon( "InputA" )]
			public int InputASquared {
				get { return InputA * InputA; }
			}

			[DependsUpon( "InputASquared" )]
			public string InputASquaredOutput {
				get { return "A Squared = " + InputASquared; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void Dependencies_Chain_Through_The_Graph() {
			var viewModel = new ChainedDependencies();
			var propertiesChanged = new List<string>();
			viewModel.PropertyChanged += ( s, e ) => propertiesChanged.Add( e.PropertyName );

			viewModel.InputA = 5;

			Assert.That( propertiesChanged.Count, Is.EqualTo( 3 ) );
			Assert.That( propertiesChanged[0], Is.EqualTo( "InputA" ) );
			Assert.That( propertiesChanged[1], Is.EqualTo( "InputASquared" ) );
			Assert.That( propertiesChanged[2], Is.EqualTo( "InputASquaredOutput" ) );
		}

		internal class DependentMethods : AutomaticCommandBase {
			public int InputA {
				get { return Get( () => InputA ); }
				set { Set( () => InputA, value ); }
			}

			public int DependentMethodExecuted { get; set; }

			[DependsUpon( "InputA" )]
			public void ExecuteWhenAChanges() {
				DependentMethodExecuted = InputA;
			}

			public void NotDependent() {
				DependentMethodExecuted++;
			}
		}

		[Test]
		public void When_InputA_Changes_Dependent_Method_Executes() {
			var viewModel = new DependentMethods { InputA = 20 };

			Assert.That( viewModel.DependentMethodExecuted, Is.EqualTo( 20 ) );
		}

		public class EmptyViewModel : AutomaticPropertyBase {
		}

		[Test]
		public void Setting_Dynamc_Properties_That_Do_Not_Exist_Can_Be_Retrieved() {
			var viewModel = new EmptyViewModel();

			viewModel.Set( "Foo", "Bar" );

			Assert.That( ( viewModel as dynamic ).Foo, Is.EqualTo( "Bar" ) );
		}

		public class InvalidDependencyCheckingForMethod : DependantPropertyBase {
			[DependsUpon( "InputA", VerifyStaticExistence = true )]
			public void ExecuteWhenAChanges() {
			}
		}

		[Test, ExpectedException( typeof( ArgumentException ) )]
		public void When_Dependant_Property_For_Method_Does_Not_Exist_And_Verification_Is_Requested_Throw() {
			new InvalidDependencyCheckingForMethod();
		}

		public class InvalidDependencyCheckingForProperty : AutomaticPropertyBase {
			[DependsUpon( "InputA", VerifyStaticExistence = true )]
			public string Derived {
				get { return string.Empty; }
			}
		}

		[Test, ExpectedException( typeof( ArgumentException ) )]
		public void When_Dependant_Propety_For_Property_Does_Not_Exist_And_Verification_Is_Requested_Throw() {
			new InvalidDependencyCheckingForProperty();
		}
	}
}
