using System;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Noise.AppSupport;
using Noise.BlobStorage.BlobStore;
using Noise.EloqueraDatabase;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using ReusableBits.Interfaces;

namespace Noise.DatabasePerformance.Tests {
	internal class LongIdentity : IIdentityProvider {
		private long			mNextIdentity;

		public	IdentityType	IdentityType { get; set; }

		public LongIdentity() {
			mNextIdentity = 0L;
		}

		public Guid NewIdentityAsGuid() {
			throw new NotImplementedException();
		}

		public long NewIdentityAsLong() {
			mNextIdentity++;

			return( mNextIdentity );
		}

		public string NewIdentityAsString() {
			throw new NotImplementedException();
		}
	}

	[TestFixture]
	public class EloqueraIdentityPerformance {
		private const int			cFirstTimeStep	= 10000;
		private const int			cSecondTimeStep = 20000;

		private readonly IUnityContainer				mContainer;
		private readonly Mock<ILibraryConfiguration>	mLibraryConfiguration;
		private readonly Mock<IBlobStorageManager>		mBlobStorageManager;
		private readonly Stopwatch						mStopWatch;
		private IEloqueraManager						mEloqueraManager;

		public EloqueraIdentityPerformance() {
			var libraryConfig = new LibraryConfiguration { DatabaseName = "Identity Performance", DatabaseServer = "localhost" };
			mLibraryConfiguration = new Mock<ILibraryConfiguration>();
			mLibraryConfiguration.Setup( m => m.Current ).Returns( libraryConfig );

			mContainer = new UnityContainer();

			var databaseModule = new EloqueraDatabaseModule( mContainer );
			databaseModule.Initialize();

			mBlobStorageManager = new Mock<IBlobStorageManager>();
			mBlobStorageManager.Setup( m => m.Initialize( It.IsAny<string>() )).Returns( true );
			mBlobStorageManager.Setup( m => m.IsOpen ).Returns( true );
			mContainer.RegisterInstance( mBlobStorageManager.Object );

			mContainer.RegisterType<IIoc, IocProvider>( new ContainerControlledLifetimeManager());

			mStopWatch = new Stopwatch();
		}

		private IEloqueraManager CreateDatabaseManager() {
			if( mEloqueraManager == null ) {
				mEloqueraManager = mContainer.Resolve<IEloqueraManager>();
				mEloqueraManager.Initialize();
			}

			return( mEloqueraManager );
		}

		private bool InitializeDatabase( IEloqueraManager databaseManager ) {
			bool retValue;

			using( var database = databaseManager.CreateDatabase()) {
				database.Database.DeleteDatabase();
			}

			using( var database = databaseManager.CreateDatabase()) {
				retValue = database.Database.OpenWithCreateDatabase();
			}

			return( retValue );
		}

		private void Teardown( IEloqueraManager databaseManager ) {
			databaseManager.ReservedDatabaseCount.Should().Be( 0 );

			using( var database = databaseManager.CreateDatabase()) {
				database.Database.DeleteDatabase();
			}
		}

		private void ReportTime( string message ) {
			Debug.WriteLine( message + mStopWatch.Elapsed );
		}

		[Test]
		public void TestIdentityPerformance() {
			DatabaseIdentityProvider.Current.IdentityType = IdentityType.Guid;
			RunIdentityPerformance();

			DatabaseIdentityProvider.Current.IdentityType = IdentityType.SequentialGuid;
			RunIdentityPerformance();

			DatabaseIdentityProvider.Current.IdentityType = IdentityType.SequentialEndingGuid;
			RunIdentityPerformance();

			Debug.WriteLine( "Replacing identity generator with long int generator." );
			DatabaseIdentityProvider.Current = new LongIdentity();
			RunIdentityPerformance();
		}

		public void RunIdentityPerformance() {
			var databaseManager = CreateDatabaseManager();

			if( InitializeDatabase( databaseManager )) {
				var trackProvider = mContainer.Resolve<ITrackProvider>();

				mStopWatch.Reset();
				ReportTime( string.Format( "Starting performance tests using identity method({0}) ", DatabaseIdentityProvider.Current.IdentityType ));
				mStopWatch.Start();

				for( int count = 0; count < cFirstTimeStep; count++ ) {
					trackProvider.AddTrack( new DbTrack());
				}

				ReportTime( string.Format( "Time after {0} track inserts: ", cFirstTimeStep ));

				for( int count = cFirstTimeStep; count < cSecondTimeStep; count++ ) {
					trackProvider.AddTrack( new DbTrack());
				}

				ReportTime( string.Format( "Time after {0} track inserts: ", cSecondTimeStep ));

				mStopWatch.Stop();
				Teardown( databaseManager );
			}
		}
	}
}
