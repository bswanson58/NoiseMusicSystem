using System;
using CuttingEdge.Conditions;
using Noise.Infrastructure;

namespace Noise.Core.FileProcessor {
	internal enum ePipelineStep {
		DetermineFileType,
		BuildMusicProviders,
		BuildInfoProviders,
		BuildArtworkProviders,
		DetermineArtist,
		DetermineAlbum,
		DetermineVolume,
		DetermineTrackName,
		AddMusicMetadata,
		UpdateMusic,
		UpdateArtwork,
		UpdateInfo,
		UpdateUndetermined,
		Completed
	}

	internal interface IPipelineStep {
		ePipelineStep	PipelineStep { get; }

		IPipelineStep	AppendStep( IPipelineStep step );
		void			ClearNextStep();
		IPipelineStep	Process( PipelineContext context );
	}

	internal abstract class BasePipelineStep : IPipelineStep {
		private IPipelineStep	mNextStep;
		public	ePipelineStep	PipelineStep { get; private set; }

		protected BasePipelineStep( ePipelineStep pipelineStep ) {
			PipelineStep = pipelineStep;
		}

		public IPipelineStep AppendStep( IPipelineStep step ) {
			Condition.Requires( step ).IsNotNull();

			if( mNextStep != null ) {
				mNextStep.AppendStep( step );
			}
			else {
				mNextStep = step;
				mNextStep.ClearNextStep();
			}

			return( this );
		}

		public void ClearNextStep() {
			mNextStep = null;
		}

		public IPipelineStep Process( PipelineContext context ) {
			var retValue = mNextStep;

			try {
				ProcessStep( context );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "StorageFile Pipeline step:", ex );
			}

			return( retValue );
		}

		public abstract void ProcessStep( PipelineContext context );
	}
}
