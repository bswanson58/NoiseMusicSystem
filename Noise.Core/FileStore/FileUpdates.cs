﻿using System;
using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Quartz;
using Quartz.Impl;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;

namespace Noise.Core.FileStore {
	internal class BackgroundFileUpdateJob : IJob {
		public void Execute( JobExecutionContext context ) {
			if( context != null ) {
				var	fileUpdater = context.Trigger.JobDataMap[FileUpdates.cBackgroundFileUpdater] as FileUpdates;

				if( fileUpdater != null ) {
					fileUpdater.ProcessUnfinishedCommands();
				}
			}
		}
	}

	internal class FileUpdates : IFileUpdates {
		internal const string					cBackgroundFileUpdater = "BackgroundFileUpdater";

		private readonly IUnityContainer		mContainer;
		private readonly INoiseManager			mNoiseManager;
		private	readonly ISchedulerFactory		mSchedulerFactory;
		private	readonly IScheduler				mJobScheduler;
		private readonly ILog					mLog;
		private readonly List<BaseCommandArgs>	mUnfinishedCommands;
		private bool							mClearReadOnly;

		private AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>			mSetRatingCommand;
		private AsyncCommand<UpdatePlayCountCommandArgs>	mUpdatePlayCountCommand;

		public FileUpdates( IUnityContainer container ) {
			mUnfinishedCommands = new List<BaseCommandArgs>();

			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();
			mJobScheduler.Start();
		}

		public bool Initialize() {
			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			mSetFavoriteCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

			mSetRatingCommand = new AsyncCommand<SetRatingCommandArgs>( OnSetRating );
			mSetRatingCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetRating.RegisterCommand( mSetRatingCommand );

			mUpdatePlayCountCommand = new AsyncCommand<UpdatePlayCountCommandArgs>( OnUpdatePlayCount );
			mUpdatePlayCountCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.UpdatePlayCount.RegisterCommand( mUpdatePlayCountCommand );

			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mClearReadOnly = configuration.EnableReadOnlyUpdates;
			}

			var jobDetail = new JobDetail( cBackgroundFileUpdater, "FileUpdates", typeof( BackgroundFileUpdateJob ));
			var trigger = new SimpleTrigger( cBackgroundFileUpdater, "FileUpdates",
											 DateTime.UtcNow + TimeSpan.FromMinutes( 1 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromMinutes( 1 )); 
			trigger.JobDataMap[cBackgroundFileUpdater] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			mLog.LogMessage( "Started Background FileUpdater." );

			return( true );
		}

		public void Shutdown() {
			if( mJobScheduler != null ) {
				mJobScheduler.Shutdown( false );
			}

			ProcessUnfinishedCommands();

			if( mUnfinishedCommands.Count > 0 ) {
				mLog.LogMessage( "FileUpdater: There were unfinished commands at shutdown." );
			}
		}

		internal void ProcessUnfinishedCommands() {
			if( mUnfinishedCommands.Count > 0 ) {
				var unfinishedCommands = new List<BaseCommandArgs>();

				lock( mUnfinishedCommands ) {
					unfinishedCommands.AddRange( mUnfinishedCommands );
					mUnfinishedCommands.Clear();
				}

				foreach( var command in unfinishedCommands ) {
					TypeSwitch.Do( command, TypeSwitch.Case<SetFavoriteCommandArgs>( OnSetFavorite ),
											TypeSwitch.Case<SetRatingCommandArgs>( OnSetRating ),
											TypeSwitch.Case<UpdatePlayCountCommandArgs>( OnUpdatePlayCount ));
				}
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var item = mNoiseManager.DataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mNoiseManager.DataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var	favoriteFrame = UserTextInformationFrame.Get( id3Tags, Constants.FavoriteFrameDescription, true );

						if( favoriteFrame != null ) {
							favoriteFrame.Text = new [] { args.Value.ToString()};

							try {
								tags.Save();
							}
							catch( Exception ) {
								mLog.LogMessage( string.Format( "FileUpdates:SetFavorite - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mNoiseManager.DataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mNoiseManager.DataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, true );

						if( popFrame != null ) {
							popFrame.Rating = StorageHelpers.ConvertToId3Rating( args.Value );

							try {
								tags.Save();
							}
							catch( Exception ) {
								mLog.LogMessage( string.Format( "FileUpdates:SetRating - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnUpdatePlayCount( UpdatePlayCountCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mNoiseManager.DataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mNoiseManager.DataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );
					var id3Tags = tags.GetTag( TagTypes.Id3v2 ) as TagLib.Id3v2.Tag;

					if( id3Tags != null ) {
						var popFrame = PopularimeterFrame.Get( id3Tags, Constants.Id3FrameUserName, true );

						if( popFrame != null ) {
							popFrame.PlayCount++;

							try {
								tags.Save();
							}
							catch( Exception ) {
								mLog.LogMessage( string.Format( "FileUpdates:UpdatePlayCount - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				mLog.LogException( "Exception - FileUpdates:OnExecutionComplete:", args.Exception );
			}
		}

		private void ClearReadOnlyFlag( string file ) {
			if( mClearReadOnly ) {
				try {
					if( System.IO.File.Exists( file )) {
					   var fileAttributes = System.IO.File.GetAttributes( file );
					
						if( fileAttributes.HasFlag( FileAttributes.ReadOnly )) {
							fileAttributes &= ~FileAttributes.ReadOnly;

							System.IO.File.SetAttributes( file, fileAttributes );
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Exception FileUpdates:ClearReadOnlyFlag for file: ({0}) -", file ), ex );
				}
			}
		}
	}
}
