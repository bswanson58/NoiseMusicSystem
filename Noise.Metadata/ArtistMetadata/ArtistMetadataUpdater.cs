using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;
using Raven.Client.Linq;
using ReusableBits.Threading;

namespace Noise.Metadata.ArtistMetadata {
	internal class ArtistMetadataUpdater : IMetadataUpdater {
		private readonly IRecurringTaskScheduler				mTaskScheduler;
		private readonly IEnumerable<IArtistMetadataProvider>	mProviders; 
		private IDocumentStore									mDocumentStore;
		private IEnumerable<DbArtistStatus>						mUpdateList;
		private IEnumerator<DbArtistStatus>						mUpdateEnumerator; 

		public ArtistMetadataUpdater( IRecurringTaskScheduler taskScheduler, IEnumerable<IArtistMetadataProvider> providers  ) {
			mTaskScheduler = taskScheduler;
			mProviders = providers;
		}

		public void Initialize( IDocumentStore documentStore ) {
			Condition.Requires( documentStore ).IsNotNull();

			mDocumentStore = documentStore;

			try {
				foreach( var provider in mProviders ) {
					provider.Initialize( mDocumentStore );
				}

				FillUpdateList();

				var updateTask = new RecurringTask( UpdateNextArtist );

				updateTask.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									   .Delay( RecurringInterval.FromSeconds( 15 ));
				mTaskScheduler.AddRecurringTask( updateTask );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ArtistMetadataUpdater:Initialize", ex );
			}
		}

		public void Shutdown() {
			mUpdateEnumerator = null;
			mUpdateList = null;
			mTaskScheduler.RemoveAllTasks();

			foreach( var provider in mProviders ) {
				provider.Shutdown();
			}

			mDocumentStore = null;
		}

		private void FillUpdateList() {
			using( var session = mDocumentStore.OpenSession()) {
				mUpdateList = ( from status in session.Query<DbArtistStatus>() select status ).ToArray();
			}

			mUpdateEnumerator = mUpdateList.GetEnumerator();
		}

		private void UpdateNextArtist( RecurringTask task ) {
			try {
				if( mUpdateEnumerator != null ) {
					if( mUpdateEnumerator.MoveNext()) {
						UpdateArtist( mUpdateEnumerator.Current );
					}
					else {
						FillUpdateList();
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ArtistMetadataUpdater:UpdateNextArtist", ex );
			}
		}

		private void UpdateArtist( DbArtistStatus artistStatus ) {
			foreach( var provider in mProviders ) {
				var status = artistStatus.GetProviderStatus( provider.ProviderKey );
				bool shouldUpdate = !(( status != null ) &&
									  ( status.LastUpdate + status.Lifetime > DateTime.Now ));

				if( shouldUpdate ) {
					provider.UpdateArtist( artistStatus.ArtistName );

					using( var session = mDocumentStore.OpenSession()) {
						artistStatus.SetLastUpdate( provider.ProviderKey );

						session.Store( artistStatus );
						session.SaveChanges();
					}
				}
			}
		}
	}
}
