using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Stateless;

namespace Noise.Core.FileProcessor {
	internal class StorageFileProcessor : IStorageFileProcessor {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private bool							mStopProcessing;

		private readonly StateMachine<ePipelineState,
							ePipelineTrigger>	mPipelineController;
		private ePipelineState					mPipelineState;
		private PipelineContext					mPipelineContext;
		private readonly Dictionary<ePipelineStep,
								IPipelineStep>	mPipelineSteps; 


		public StorageFileProcessor( IArtistProvider artistProvider, IAlbumProvider albumProvider,
									 IStorageFileProvider fileProvider, IStorageFolderSupport storageFolderSupport,
								     IEnumerable<IPipelineStep> pipelineSteps  ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mStorageFileProvider = fileProvider;
			mStorageFolderSupport = storageFolderSupport;

			mPipelineSteps = new Dictionary<ePipelineStep, IPipelineStep>();
			pipelineSteps.ForEach( step => mPipelineSteps.Add( step.PipelineStep, step ));

			mPipelineController = new StateMachine<ePipelineState, ePipelineTrigger>( () => mPipelineState, newState => mPipelineState = newState );
			mPipelineState = ePipelineState.Stopped;

			mPipelineController.Configure( ePipelineState.Stopped )
				.OnEntry(() => OnStopped( mPipelineContext ))
				.Permit( ePipelineTrigger.DetermineFileType, ePipelineState.DetermineFileState );

			mPipelineController.Configure( ePipelineState.DetermineFileState )
				.OnEntry(() => DetermineFileType( mPipelineContext ))
				.Permit( ePipelineTrigger.FileTypeIsAudio, ePipelineState.BuildAudioFile )
				.Permit( ePipelineTrigger.FileTypeIsArtwork, ePipelineState.BuildArtworkFile )
				.Permit( ePipelineTrigger.FileTypeIsInfo, ePipelineState.BuildInfoFile )
				.Permit( ePipelineTrigger.FileTypeIsUnknown, ePipelineState.BuildUnknownFile );

			mPipelineController.Configure( ePipelineState.BuildAudioFile )
				.OnEntry(() => ProcessAudioFile( mPipelineContext ))
				.Permit( ePipelineTrigger.Completed, ePipelineState.Stopped );

			mPipelineController.Configure( ePipelineState.BuildArtworkFile )
				.OnEntry(() => ProcessArtworkFile( mPipelineContext ))
				.Permit( ePipelineTrigger.Completed, ePipelineState.Stopped );

			mPipelineController.Configure( ePipelineState.BuildInfoFile )
				.OnEntry(() => ProcessInfoFile( mPipelineContext ))
				.Permit( ePipelineTrigger.Completed, ePipelineState.Stopped );

			mPipelineController.Configure( ePipelineState.BuildUnknownFile )
				.OnEntry(() => ProcessUndeterminedFile( mPipelineContext ))
				.Permit( ePipelineTrigger.Completed, ePipelineState.Stopped );
		}

		private void DetermineFileType( PipelineContext context ) {
			context.SetPipeline( mPipelineSteps[ePipelineStep.DetermineFileType]);
		}

		private void ProcessAudioFile( PipelineContext context ) {
			context.SetPipeline( mPipelineSteps[ePipelineStep.BuildMusicProviders]
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineArtist])
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineAlbum])
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineTrackName])
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineVolume])
									.AppendStep( mPipelineSteps[ePipelineStep.AddMusicMetadata])
									.AppendStep( mPipelineSteps[ePipelineStep.UpdateMusic])
									.AppendStep( mPipelineSteps[ePipelineStep.Completed]));
		}

		private void ProcessArtworkFile( PipelineContext context ) {
			context.SetPipeline( mPipelineSteps[ePipelineStep.BuildArtworkProviders]
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineArtist])
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineAlbum])
									.AppendStep( mPipelineSteps[ePipelineStep.UpdateArtwork])
									.AppendStep( mPipelineSteps[ePipelineStep.Completed]));
		}

		private void ProcessInfoFile( PipelineContext context ) {
			context.SetPipeline( mPipelineSteps[ePipelineStep.BuildInfoProviders]
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineArtist])
									.AppendStep( mPipelineSteps[ePipelineStep.DetermineAlbum])
									.AppendStep( mPipelineSteps[ePipelineStep.UpdateInfo])
									.AppendStep( mPipelineSteps[ePipelineStep.Completed]));
		}

		private void ProcessUndeterminedFile( PipelineContext context ) {
			context.SetPipeline( mPipelineSteps[ePipelineStep.UpdateUndetermined]
									.AppendStep( mPipelineSteps[ePipelineStep.Completed]));
		}

		private void OnStopped( PipelineContext context ) {
			if( context != null ) {
				context.ClearPipeline();
			}
		}

		public void Process( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();

			mStopProcessing = false;
			
			try {
				using( var fileList = mStorageFileProvider.GetFilesRequiringProcessing()) {
					DatabaseCache<DbArtist>	artistCache;
					DatabaseCache<DbAlbum>	albumCache;

					using( var artistList = mArtistProvider.GetArtistList()) {
						artistCache = new DatabaseCache<DbArtist>( artistList.List );
					}
					using( var albumList = mAlbumProvider.GetAllAlbums()) {
						albumCache = new DatabaseCache<DbAlbum>( albumList.List );
					}

					var fileEnum = from file in fileList.List orderby file.ParentFolder select file;
					foreach( var file in fileEnum ) {
						try {
							mPipelineContext = new PipelineContext( artistCache, albumCache, file, summary );
							mPipelineController.Fire( ePipelineTrigger.DetermineFileType );

							while( mPipelineContext.PipelineStep != null ) {
								var nextStep = mPipelineContext.PipelineStep;

								while( nextStep != null ) {
									nextStep = nextStep.Process( mPipelineContext );
								}

								mPipelineController.Fire( mPipelineContext.Trigger );
							}
						}
						catch( Exception ex ) {
							NoiseLogger.Current.LogException( string.Format( "StorageFileProcessor processing:{0}",
																mStorageFolderSupport.GetPath( file )), ex );
						}

						if( mStopProcessing ) {
							break;
						}
					}

					artistCache.Clear();
					albumCache.Clear();
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Processing StorageFile pipeline:", ex );
			}
		}

		public void Stop() {
			mStopProcessing = true;
		}
	}
}
