using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Noise.Infrastructure;

namespace Noise.Core {
	public class NoiseManager {
		private	string							mDatabaseName;
		private string							mDatabaseLocation;
		private readonly CompositionContainer	mContainer;

		[Import]
		private ILog				mLog;
		[Import]
		private IDatabaseManager	mDatabase;

		public NoiseManager() {
			var catalog = new DirectoryCatalog( @".\" );

			mContainer = new CompositionContainer( catalog );
			mContainer.ComposeParts( this );
		}

		public bool Initialize() {
			LoadConfiguration();
			if( mDatabase.InitializeDatabase( mDatabaseLocation ) ) {
				mDatabase.OpenWithCreateDatabase( mDatabaseName );
			}

			mLog.LogMessage( "Initialized NoiseManager." );

			return ( true );
		}

		private void LoadConfiguration() {
			mDatabaseLocation = "(local)";
			mDatabaseName = "Noise";
		}
	}
}
