using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;
using Raven.Client;
using Raven.Client.Embedded;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization, IMetadataManager,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IArtistMetadataManager			mArtistMetadataManager;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IEnumerable<IMetadataUpdater>	mUpdaters; 
		private IDocumentStore							mDocumentStore;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator, IEnumerable<IMetadataUpdater> updaters,
								IArtistMetadataManager artistMetadataManager, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mUpdaters = updaters;
			mArtistMetadataManager = artistMetadataManager;
			mArtistProvider = artistProvider;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
			try {
				var libraryPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
												Constants.CompanyName, 
												Constants.LibraryConfigurationDirectory,
												"Metadata" );
				mDocumentStore = new EmbeddableDocumentStore { DataDirectory = libraryPath };
				mDocumentStore.Initialize();
				mArtistMetadataManager.Initialize( mDocumentStore );

				foreach( var updater in mUpdaters ) {
					updater.Initialize( mDocumentStore );
				}

				mEventAggregator.Subscribe( this );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "MetadataManager:Initialize", ex );
			}
		}

		public void Shutdown() {
			foreach( var updater in mUpdaters ) {
				updater.Shutdown();
			}

			mDocumentStore.Dispose();
		}

		public void Handle( Events.ArtistAdded args ) {
			var artist = mArtistProvider.GetArtist( args.ArtistId );

			if( artist != null ) {
				mArtistMetadataManager.ArtistMentioned( artist.Name );
			}
		}

		public void Handle( Events.ArtistRemoved args ) {
			var artist = mArtistProvider.GetArtist( args.ArtistId );

			if( artist != null ) {
				mArtistMetadataManager.ArtistForgotten( artist.Name );
			}
		}

		public IArtistMetadata GetArtistMetadata( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistBiography( forArtist ));
		}

		public IArtistDiscography GetArtistDiscography( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistDiscography( forArtist ));
		}
	}
}
