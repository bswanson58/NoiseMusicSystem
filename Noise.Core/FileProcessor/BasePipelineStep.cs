using System;
using CuttingEdge.Conditions;
using Noise.Infrastructure;

namespace Noise.Core.FileProcessor {
	public enum ePipelineStep {
		DetermineFileType,
		BuildMusicProviders,
		BuildInfoProviders,
		BuildArtworkProviders,
		BuildSidecarProviders,
		DetermineArtist,
		DetermineAlbum,
		DetermineVolume,
		DetermineTrackName,
		AddMusicMetadata,
		UpdateMusic,
		UpdateArtwork,
		UpdateInfo,
		UpdateSidecar,
		UpdateUndetermined,
		Completed
	}

	public interface IPipelineStep {
		ePipelineStep	PipelineStep { get; }

		IPipelineStep	AppendStep( IPipelineStep step );
		void			ClearNextStep();
		IPipelineStep	Process( PipelineContext context );
	}

	public abstract class BasePipelineStep : IPipelineStep {
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

			ProcessStep( context );

			return( retValue );
		}

		public abstract void ProcessStep( PipelineContext context );
	}
}
