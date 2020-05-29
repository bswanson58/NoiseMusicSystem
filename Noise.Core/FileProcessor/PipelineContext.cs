using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.DataProviders;
using Noise.Core.Database;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;

namespace Noise.Core.FileProcessor {
	public class PipelineContext {
		public	ILogLibraryClassification	Log {  get; private set; }
		public	DatabaseCache<DbArtist>		ArtistCache { get; private set; }
		public	DatabaseCache<DbAlbum>		AlbumCache { get; private set; }
		public	List<IMetadataInfoProvider>		MetaDataProviders { get; set; }
		public	DatabaseChangeSummary		Summary { get; private set; }

		public	StorageFile					StorageFile { get; private set; }
		public	DbArtist					Artist { get; set; }
		public	DbAlbum						Album { get; set; }
		public	DbTrack						Track { get; set; }

		public	IPipelineStep				PipelineStep { get; private set; }
		public	ePipelineTrigger			Trigger { get; set; }

		public PipelineContext( DatabaseCache<DbArtist> artistCache, DatabaseCache<DbAlbum> albumCache,
								StorageFile file, DatabaseChangeSummary summary, ILogLibraryClassification log ) {
			Log = log;
			ArtistCache = artistCache;
			AlbumCache = albumCache;
			StorageFile = file;
			Summary = summary;
		}

		public void SetPipeline( IPipelineStep step ) {
			Condition.Requires( step ).IsNotNull();

			PipelineStep = step;
			Trigger = ePipelineTrigger.Unexpected;
		}

		public void ClearPipeline() {
			PipelineStep = null;
			Trigger = ePipelineTrigger.Unexpected;
		}
	}
}
