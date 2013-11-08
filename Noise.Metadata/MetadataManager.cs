using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;
using Raven.Abstractions.Smuggler;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Database.Server;
using Raven.Database.Smuggler;

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
				string metaDirectory = "Metadata";
#if DEBUG
				metaDirectory += " (Debug)";
#endif
				var libraryPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
												Constants.CompanyName, 
												Constants.LibraryConfigurationDirectory,
												metaDirectory );
				mDocumentStore = new EmbeddableDocumentStore { DataDirectory = libraryPath };
#if DEBUG
				try {
					( mDocumentStore as EmbeddableDocumentStore ).UseEmbeddedHttpServer = true;
					NonAdminHttp.EnsureCanListenToWhenInNonAdminContext( 8080 );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "RavenDB Embedded HTTP Server could not be initialized", ex );

					( mDocumentStore as EmbeddableDocumentStore ).UseEmbeddedHttpServer = false;
				}
#endif
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

		public Artwork GetArtistArtwork( string forArtist ) {
			mArtistMetadataManager.ArtistMentioned( forArtist );
			mUpdaters.Apply( updater => updater.QueueArtistUpdate( forArtist ));

			return( mArtistMetadataManager.GetArtistArtwork( forArtist ));
		}

		public void ExportMetadata( string exportName ) {
			try {
				var exportPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
												Constants.CompanyName, 
												Constants.LibraryConfigurationDirectory,
												exportName );
				if( mDocumentStore is EmbeddableDocumentStore ) {
					var embeddedStore = mDocumentStore as EmbeddableDocumentStore;
					var options = new SmugglerOptions { BackupPath = exportPath };
					var exporter = new DataDumper( embeddedStore.DocumentDatabase, options );

					exporter.ExportData( options );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "MetadataManager:ExportMetadata", ex );
			}
		}

		public void ImportMetadata( string importName ) {
			try {
				var exportPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
												Constants.CompanyName, 
												Constants.LibraryConfigurationDirectory,
												importName );
				if( mDocumentStore is EmbeddableDocumentStore ) {
					var embeddedStore = mDocumentStore as EmbeddableDocumentStore;
					var options = new SmugglerOptions { BackupPath = exportPath };
					var exporter = new DataDumper( embeddedStore.DocumentDatabase, options );

					exporter.ImportData( options );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "MetadataManager:ImportMetadata", ex );
			}
		}
	}
}
