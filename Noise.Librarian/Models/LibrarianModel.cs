using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;

namespace Noise.Librarian.Models {
	public class LibrarianModel : ILibrarian {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILifecycleManager		mLifecycleManager;
		private readonly IDatabaseManager		mDatabaseManager;

		public LibrarianModel( IEventAggregator eventAggregator,
							   ILifecycleManager lifecycleManager,
							   IDatabaseManager databaseManager ) {
			mEventAggregator = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mDatabaseManager = databaseManager;
		}

		public bool Initialize() {
			NoiseLogger.Current.LogMessage( "Initializing Noise Music System - Librarian" );

			try {
				mLifecycleManager.Initialize();

				NoiseLogger.Current.LogMessage( "Initialized LibrarianModel." );

				mEventAggregator.Publish( new Events.SystemInitialized());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibrarianModel:Initialize", ex );
			}

			return( true );
		}

		public void Shutdown() {
			mEventAggregator.Publish( new Events.SystemShutdown());

			mLifecycleManager.Shutdown();
			mDatabaseManager.Shutdown();

			NoiseLogger.Current.LogMessage( "Shutdown LibrarianModel." );
		}
	}
}
