﻿using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;
using Raven.Abstractions.Smuggler;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Smuggler;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization, IMetadataManager,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly INoiseLog						mLog;
		private readonly INoiseEnvironment				mNoiseEnvironment;
		private readonly IArtistMetadataManager			mArtistMetadataManager;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IEnumerable<IMetadataUpdater>	mUpdaters; 
		private IDocumentStore							mDocumentStore;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator, INoiseEnvironment noiseEnvironment,
								IMetadataUpdater[] updaters, IArtistMetadataManager artistMetadataManager, IArtistProvider artistProvider, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mNoiseEnvironment = noiseEnvironment;
			mUpdaters = updaters;
			mArtistMetadataManager = artistMetadataManager;
			mArtistProvider = artistProvider;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
			try {
				string metaDirectory = Constants.MetadataDirectory;
#if DEBUG
				metaDirectory += " (Debug)";
#endif
				var libraryPath = Path.Combine( mNoiseEnvironment.LibraryDirectory(), metaDirectory );
				mDocumentStore = new EmbeddableDocumentStore { DataDirectory = libraryPath };
				mDocumentStore.Conventions.DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite;
/*
				try {
					( mDocumentStore as EmbeddableDocumentStore ).UseEmbeddedHttpServer = true;
					NonAdminHttp.EnsureCanListenToWhenInNonAdminContext( 8080 );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "RavenDB Embedded HTTP Server could not be initialized", ex );

					( mDocumentStore as EmbeddableDocumentStore ).UseEmbeddedHttpServer = false;
				}
 */
				mDocumentStore.Initialize();
				mArtistMetadataManager.Initialize( mDocumentStore );

				foreach( var updater in mUpdaters ) {
					updater.Initialize( mDocumentStore );
				}

				mEventAggregator.Subscribe( this );
			}
			catch( Exception ex ) {
				mLog.LogException( "Failed to initialize", ex );
			}
		}

		public void Shutdown() {
			foreach( var updater in mUpdaters ) {
				updater.Shutdown();
			}

			mDocumentStore?.Dispose();
			mDocumentStore = null;
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

        public bool ArtistPortfolioAvailable( string forArtist ) {
            return mArtistMetadataManager.ArtistPortfolioAvailable( forArtist );
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

        public IEnumerable<Artwork> GetArtistPortfolio( string forArtist ) {
            return mArtistMetadataManager.GetArtistPortfolio( forArtist );
        }

        public async void ExportMetadata( string exportPath ) {
			try {
				if( mDocumentStore is EmbeddableDocumentStore ) {
                    var options = new SmugglerOptions();

                    if( mDocumentStore is EmbeddableDocumentStore embeddedStore ) {
                        var exporter = new DataDumper( embeddedStore.DocumentDatabase, options );

                        using( var stream = File.OpenWrite( exportPath )) {

                            await exporter.ExportData( stream, options, false );
                        }
                    }
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Exporting Metadata to \"{exportPath}\"", ex );
			}
		}

		public async void ImportMetadata( string importPath ) {
			try {
				if(( mDocumentStore is EmbeddableDocumentStore ) &&
				   ( File.Exists( importPath ))) {
                    var options = new SmugglerOptions();

                    if( mDocumentStore is EmbeddableDocumentStore embeddedStore ) {
					    var importer = new DataDumper( embeddedStore.DocumentDatabase, options );

					    using( var stream = File.OpenRead( importPath )) {
						    await importer.ImportData( stream, options );
					    }
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Importing Metadata from \"{importPath}\"", ex );
			}
		}
	}
}
