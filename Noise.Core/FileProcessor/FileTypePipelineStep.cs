﻿using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class FileTypePipelineStep : BasePipelineStep {
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly ITrackProvider			mTrackProvider;

		public FileTypePipelineStep( IStorageFolderSupport storageFolderSupport, ITrackProvider trackProvider ) :
			base( ePipelineStep.DetermineFileType ) {
			mStorageFolderSupport = storageFolderSupport;
			mTrackProvider = trackProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			context.StorageFile.FileType = mStorageFolderSupport.DetermineFileType( context.StorageFile );
			context.Log.LogFileTypeDetermined( context.StorageFile );

			switch( context.StorageFile.FileType ) {
				case eFileType.Music:
					context.Trigger = ePipelineTrigger.FileTypeIsAudio;
					context.Track = context.StorageFile.WasUpdated ? mTrackProvider.GetTrack( context.StorageFile.MetaDataPointer ) :
																	 new DbTrack { Encoding = mStorageFolderSupport.DetermineAudioEncoding( context.StorageFile )};
					break;

				case eFileType.Picture:
					context.Trigger = ePipelineTrigger.FileTypeIsArtwork;
					break;

				case eFileType.Text:
					context.Trigger = ePipelineTrigger.FileTypeIsInfo;
					break;

				case eFileType.Sidecar:
					context.Trigger = ePipelineTrigger.FileTypeIsSidecar;
					break;

				default:
					context.Trigger = ePipelineTrigger.FileTypeIsUnknown;
					break;
			}
		}
	}
}
