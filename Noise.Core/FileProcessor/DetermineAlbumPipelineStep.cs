using System;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class DetermineAlbumPipelineStep : BasePipelineStep {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IAlbumProvider			mAlbumProvider;

		public DetermineAlbumPipelineStep( IEventAggregator eventAggregator, IAlbumProvider albumProvider ) :
			base( ePipelineStep.DetermineAlbum ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.Summary ).IsNotNull();

			if( context.Artist != null ) {
				var	albumName = string.Empty;

				foreach( var provider in context.MetaDataProviders ) {
					albumName = provider.Album;

					if(!string.IsNullOrWhiteSpace( albumName )) {
						break;
					}
				}

				if(!string.IsNullOrWhiteSpace( albumName )) {
					if(!albumName.Equals( Constants.LibraryMetadataFolder, StringComparison.InvariantCultureIgnoreCase )) {
						context.Album = context.AlbumCache.Find( album => album.Name == albumName && album.Artist == context.Artist.DbId );
						if( context.Album == null ) {
							context.Album = new DbAlbum { Name = albumName, Artist = context.Artist.DbId };

							mAlbumProvider.AddAlbum( context.Album );
							context.AlbumCache.Add( context.Album );

							context.Artist.AlbumCount++;
							context.Summary.AlbumsAdded++;

							mEventAggregator.PublishOnUIThread( new Events.AlbumAdded( context.Album.DbId ));
							context.Log.LogAlbumAdded( context.StorageFile, context.Album );
						}
						else {
							context.Log.LogAlbumFound( context.StorageFile, context.Album );
						}
					}
				}
				else {
					context.Log.LogAlbumNotFound( context.StorageFile );
				}
			}
		}
	}
}
