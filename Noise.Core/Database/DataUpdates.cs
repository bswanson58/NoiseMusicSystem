using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.Database {
	internal class DataUpdates : IDataUpdates, IRequireInitialization {
		private readonly INoiseLog			mLog;
		private readonly IArtworkProvider	mArtworkProvider;

		private AsyncCommand<SetAlbumCoverCommandArgs>	mSetAlbumCoverCommand;

		public DataUpdates( ILifecycleManager lifecycleManager, INoiseLog log,
							IArtworkProvider artworkProvider ) {
			mLog = log;
			mArtworkProvider = artworkProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			mSetAlbumCoverCommand = new AsyncCommand<SetAlbumCoverCommandArgs>( OnSetAlbumCover );
			mSetAlbumCoverCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetAlbumCover.RegisterCommand( mSetAlbumCoverCommand );
		}

		public void Shutdown() {
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				mLog.LogException( "After command execution", args.Exception );
			}
		}

		private void OnSetAlbumCover( SetAlbumCoverCommandArgs args ) {
			try {
				var artworkList = mArtworkProvider.GetAlbumArtwork( args.AlbumId );
				foreach( var artwork in artworkList ) {
					if(( artwork.IsUserSelection ) &&
					   ( artwork.DbId != args.ArtworkId )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = false;

								artworkUpdater.Update();
							}
						}
					}

					if(( artwork.DbId == args.ArtworkId ) &&
					   (!artwork.IsUserSelection )) {
						using( var artworkUpdater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
							if( artworkUpdater.Item != null ) {
								artworkUpdater.Item.IsUserSelection = true;

								artworkUpdater.Update();
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Setting album cover {0} for Album {1}", args.ArtworkId, args.AlbumId ), ex );
			}
		}
	}
}
