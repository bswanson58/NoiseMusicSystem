using System.Configuration;
using NUnit.Framework;

namespace ReusableBits.Tests.FeatureToggle.Tests {
	[TestFixture]
	public class BooleanSqlServerProviderTests {
		[Test]
		public void ShouldReadBooleanTrueFromSqlServer() {
			var sut = new BooleanSqlServerProvider();

			var toggle = new MySqlServerToggleTrue();

			Assert.IsTrue( sut.EvaluateBooleanToggleValue( toggle ) );
		}


		[Test]
		public void ShouldReadBooleanFalseFromSqlServer() {
			var sut = new BooleanSqlServerProvider();

			var toggle = new MySqlServerToggleFalse();

			Assert.IsTrue( sut.EvaluateBooleanToggleValue( toggle ) );
		}



		[Test, ExpectedException( typeof( ConfigurationErrorsException ) )]
		public void ShouldErrorWhenConnectionsStringNotInConfig() {
			var sut = new MissingConnectionStringSqlServerToggle();

			var x = sut.FeatureEnabled;
		}


		[Test, ExpectedException( typeof( ConfigurationErrorsException ) )]
		public void ShouldErrorWhenSqlStatementNotInConfig() {
			var sut = new MissingSqlStatementSqlServerToggle();

			var x = sut.FeatureEnabled;
		}



		private class MySqlServerToggleTrue : SqlFeatureToggle { }
		private class MySqlServerToggleFalse : SqlFeatureToggle { }
		private class MissingConnectionStringSqlServerToggle : SqlFeatureToggle { }
		private class MissingSqlStatementSqlServerToggle : SqlFeatureToggle { }
	}
}
