using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.EventMonitoring;
using NUnit.Framework;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ReusableBits.Mvvm.Tests.ViewModelSupport {
	[TestFixture]
	public class AutomaticPropertyBaseTests {
		private class ValuesByString : AutomaticPropertyBase {
			public const int cIntegerDefaultValue = 13;
			public bool	     SetReturnValue { get; private set; }

			public string StringValue {
				get { return Get<string>( "StringValue" ); }
				set { SetReturnValue = Set( "StringValue", value ); }
			}

			public int IntegerValue {
				get { return Get<int>( "IntegerValue" ); }
				set { SetReturnValue = Set( "IntegerValue", value ); }
			}

			public int IntegerWithDefault {
				get { return Get( "IntegerWithDefault", cIntegerDefaultValue ); }
				set { SetReturnValue = Set( "IntegerWithDefault", value ); }
			}
		} 

		[Test]
		public void ValuseSetByStringCanBeRetrieved() {
			const string testValue = "test string";
			var viewModel = new ValuesByString { StringValue = testValue };

			Assert.That( viewModel.StringValue, Is.EqualTo( testValue ) );
		}

		[Test]
		public void IntegerValuesCanBeSet() {
			const int testValue = 33;
			var viewModel = new ValuesByString { IntegerValue = testValue };

			Assert.That( viewModel.IntegerValue, Is.EqualTo( testValue ));
		}

		[Test]
		public void SettingValueFiresPropertyChanged() {
			var sut = new ValuesByString();
			sut.MonitorEvents();

			sut.StringValue = "test string";

			sut.ShouldRaisePropertyChangeFor( p => p.StringValue );
		}

		[Test]
		public void ValueCanBeSetTwice() {
			const string testValue = "test me";
			var sut = new ValuesByString { StringValue = testValue };

			sut.StringValue = testValue;

			Assert.That( sut.StringValue, Is.EqualTo( testValue ) );
		}

		[Test]
		public void SettingValueReturnsTrue() {
			var sut = new ValuesByString();

			sut.StringValue = "test value";

			Assert.That( sut.SetReturnValue, Is.True );
		}

		[Test]
		public void SettingSameValueFiresSinglePropertyChanged() {
			const string testValue = "string by two";
			var sut = new ValuesByString { StringValue = testValue };

			sut.MonitorEvents();

			sut.StringValue = testValue;

			sut.ShouldNotRaisePropertyChangeFor( p => p.StringValue );
		}

		[Test]
		public void SettingSameValueReturnsFalse() {
			const string testValue = "twice";
			var sut = new ValuesByString { StringValue = testValue };

			sut.StringValue = testValue;

			Assert.That( sut.SetReturnValue, Is.False );
		}

		[Test]
		public void SettingSameValueToNullFiresSinglePropertyChange() {
			const string testValue = "string by two";
			var sut = new ValuesByString { StringValue = testValue };

			sut.MonitorEvents();

			sut.StringValue = null;

			sut.ShouldRaisePropertyChangeFor( p => p.StringValue );
		}

		private class ViewModelWithMismatchedPropertyNames : AutomaticPropertyBase {
			public int Value {
				get { return Get<int>( "WrongName" ); }
				set { Set( "Value", value ); }
			}
		}

		[Test]
		public void MismatchedPropertiesReturnsDefault() {
			var sut = new ViewModelWithMismatchedPropertyNames { Value = 55 };

			Assert.That( sut.Value, Is.EqualTo( default( int )));
		}

		[Test]
		public void DefaultValuesAreReturned() {
			var sut = new ValuesByString();

			Assert.That( sut.IntegerWithDefault, Is.EqualTo( ValuesByString.cIntegerDefaultValue ));
		}

		[Test]
		public void SettingWithDefaultReturnsSetValue() {
			const int defaultInteger = 77;
			var sut = new ValuesByString { IntegerWithDefault = defaultInteger };

			Assert.That( sut.IntegerWithDefault, Is.EqualTo( defaultInteger ));
		}

		public class ValuesByLambda : AutomaticPropertyBase {
			public const int cDefaultIntegerValue = 11;
			public bool      SetReturnValue { get; private set; }

			public string StringValue {
				get { return Get( () => StringValue ); }
				set { SetReturnValue = Set( () => StringValue, value ); }
			}

			public int IntegerValueWithDefault {
				get { return Get( () => IntegerValueWithDefault, cDefaultIntegerValue ); }
				set { SetReturnValue = Set( () => IntegerValueWithDefault, value ); }
			}
		}

		[Test]
		public void LambdaPropertyCanBeUsed() {
			const string testValue = "another string";
			var sut = new ValuesByLambda { StringValue = testValue };

			Assert.That( sut.StringValue, Is.EqualTo( testValue ));
		}

		[Test]
		public void LambdaPropertyUsesDefaultValue() {
			var sut = new ValuesByLambda();

			Assert.That( sut.IntegerValueWithDefault, Is.EqualTo( ValuesByLambda.cDefaultIntegerValue ));
		}

		[Test]
		public void LambdaSettingValueReturnsTrue() {
			var sut = new ValuesByLambda();

			sut.StringValue = "test value";

			Assert.That( sut.SetReturnValue, Is.True );
		}

		private class InitialValueOnProperties : AutomaticPropertyBase {
			public const string cDefaultStringValue = "default value";
			public int InitialValueCount;

			private string InitialValue() {
				InitialValueCount++;

				return( cDefaultStringValue );
			}

			public string TestProperty {
				get { return Get( () => TestProperty, InitialValue ); }
			}

			public string TestStringProperty {
				get { return Get( "TestStringProperty", InitialValue ); }
			}
		}

		[Test]
		public void InitialValueSetOnStringGetter() {
			var sut = new InitialValueOnProperties();

			var value = sut.TestStringProperty;

			Assert.That( value, Is.EqualTo( InitialValueOnProperties.cDefaultStringValue ));
		}

		[Test]
		public void InitialValueSetOnLambdaGetter() {
			var sut = new InitialValueOnProperties();

			var value = sut.TestProperty;

			Assert.That( value, Is.EqualTo( InitialValueOnProperties.cDefaultStringValue ));
		}

		[Test]
		public void InitialValueOnlyRequestedOnce() {
			var sut = new InitialValueOnProperties();

			sut.InitialValueCount.Should().Be( 0 );

			var value = sut.TestProperty;
			sut.InitialValueCount.Should().Be( 1 );
			value.Should().Be( InitialValueOnProperties.cDefaultStringValue );

			value = sut.TestProperty;
			sut.InitialValueCount.Should().Be( 1 );
			value.Should().Be( InitialValueOnProperties.cDefaultStringValue );
		}

		internal class SingleDependency : AutomaticPropertyBase {
			public int BaseProperty {
				get { return Get( () => BaseProperty ); }
				set { Set( () => BaseProperty, value ); }
			}

			[DependsUpon( "BaseProperty" )]
			public int DependantProperty {
				get { return BaseProperty * BaseProperty; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void BasePropertyFiresDependantProperty() {
			var sut = new SingleDependency();
			sut.MonitorEvents();

			sut.BaseProperty = 5;

			sut.ShouldRaisePropertyChangeFor( p => p.BaseProperty );
			sut.ShouldRaisePropertyChangeFor( p => p.DependantProperty );
		}

		internal class MultipleDependencies : AutomaticPropertyBase {
			public int BasePropertyOne {
				get { return Get( () => BasePropertyOne ); }
				set { Set( () => BasePropertyOne, value ); }
			}

			public int BasePropertyTwo {
				get { return Get( () => BasePropertyTwo ); }
				set { Set( () => BasePropertyTwo, value ); }
			}

			[DependsUpon( "BasePropertyOne" )]
			[DependsUpon( "BasePropertyTwo" )]
			public int DependantProperty {
				get { return BasePropertyOne + BasePropertyTwo; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void BasePropertyFiresMultipleDependantProperties() {
			var sut = new MultipleDependencies();
			var propertiesChanged = new List<string>();
			sut.PropertyChanged += ( s, e ) => propertiesChanged.Add( e.PropertyName );

			sut.BasePropertyOne = 5;
			sut.BasePropertyTwo = 6;

			Assert.That( propertiesChanged.Count, Is.EqualTo( 4 ) );
			Assert.That( propertiesChanged[0], Is.EqualTo( "BasePropertyOne" ) );
			Assert.That( propertiesChanged[1], Is.EqualTo( "DependantProperty" ) );
			Assert.That( propertiesChanged[2], Is.EqualTo( "BasePropertyTwo" ) );
			Assert.That( propertiesChanged[3], Is.EqualTo( "DependantProperty" ) );
		}

		internal class MultiplePropertiesSingleDependency : AutomaticPropertyBase {
			public int BaseProperty {
				get { return Get( () => BaseProperty ); }
				set { Set( () => BaseProperty, value ); }
			}

			[DependsUpon( "BaseProperty" )]
			public int DependantPropertyOne {
				get { return BaseProperty * BaseProperty; }
			}

			[DependsUpon( "BaseProperty" )]
			public int DependantPropertyTwo {
				get { return BaseProperty * BaseProperty * BaseProperty; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void When_InputA_Changes_All_Dependent_Notifications_Fires() {
			var sut = new MultiplePropertiesSingleDependency();
			sut.MonitorEvents();

			sut.BaseProperty = 5;

			sut.ShouldRaisePropertyChangeFor( p => p.BaseProperty );
			sut.ShouldRaisePropertyChangeFor( p => p.DependantPropertyOne );
			sut.ShouldRaisePropertyChangeFor( p => p.DependantPropertyTwo );
		}

		internal class ChainedDependencies : AutomaticPropertyBase {
			public int BaseProperty {
				get { return Get( () => BaseProperty ); }
				set { Set( () => BaseProperty, value ); }
			}

			[DependsUpon( "BaseProperty" )]
			public int DependantPropertyOne {
				get { return BaseProperty * BaseProperty; }
			}

			[DependsUpon( "DependantPropertyOne" )]
			public string DependantPropertyTwo {
				get { return "dependant propert two: " + DependantPropertyOne; }
			}

			public int NotDependent {
				get { return 20; }
			}
		}

		[Test]
		public void DependentPropertiesFirePropertyChanged() {
			var sut = new ChainedDependencies();
			sut.MonitorEvents();

			sut.BaseProperty = 5;

			sut.ShouldRaisePropertyChangeFor( p => p.BaseProperty );
			sut.ShouldRaisePropertyChangeFor( p => p.DependantPropertyOne );
			sut.ShouldRaisePropertyChangeFor( p => p.DependantPropertyTwo );
		}

		public class InvalidDependencyProperty : AutomaticPropertyBase {
			[DependsUpon( "BasePropertyOne", VerifyStaticExistence = true )]
			public string Derived {
				get { return string.Empty; }
			}
		}

		[Test, ExpectedException( typeof( ArgumentException ) )]
		public void InvalidPropertyDependancyThrowsException() {
			new InvalidDependencyProperty();
		}
	}
}
