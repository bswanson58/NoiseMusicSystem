using System;
using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
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

		private readonly IDataProvider			mDataProvider;
		private	readonly ISchedulerFactory		mSchedulerFactory;
		private	readonly IScheduler				mJobScheduler;
		private readonly List<BaseCommandArgs>	mUnfinishedCommands;
		private bool							mClearReadOnly;

		private AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;
		private AsyncCommand<SetRatingCommandArgs>			mSetRatingCommand;
		private AsyncCommand<UpdatePlayCountCommandArgs>	mUpdatePlayCountCommand;
		private AsyncCommand<SetMp3TagCommandArgs>			mSetMp3TagsCommand;

		public FileUpdates( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;

			mUnfinishedCommands = new List<BaseCommandArgs>();
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

			mSetMp3TagsCommand = new AsyncCommand<SetMp3TagCommandArgs>( OnSetMp3Tags );
			mSetMp3TagsCommand.ExecutionComplete += OnExecutionComplete;
			GlobalCommands.SetMp3Tags.RegisterCommand( mSetMp3TagsCommand );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mClearReadOnly = configuration.EnableReadOnlyUpdates;
			}

			var jobDetail = new JobDetail( cBackgroundFileUpdater, "FileUpdates", typeof( BackgroundFileUpdateJob ));
			var trigger = new SimpleTrigger( cBackgroundFileUpdater, "FileUpdates",
											 DateTime.UtcNow + TimeSpan.FromMinutes( 1 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromMinutes( 1 )); 
			trigger.JobDataMap[cBackgroundFileUpdater] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			NoiseLogger.Current.LogMessage( "Started Background FileUpdater." );

			return( true );
		}

		public void Shutdown() {
			if( mJobScheduler != null ) {
				mJobScheduler.Shutdown( false );
			}

			ProcessUnfinishedCommands();

			if( mUnfinishedCommands.Count > 0 ) {
				NoiseLogger.Current.LogMessage( "FileUpdater: There were unfinished commands at shutdown." );
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
											TypeSwitch.Case<SetMp3TagCommandArgs>( SetMp3FileTags ),
											TypeSwitch.Case<UpdatePlayCountCommandArgs>( OnUpdatePlayCount ));
				}
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();

			var item = mDataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mDataProvider.GetPhysicalFile( track );

				if( file != null ) {
					switch( StorageHelpers.DetermineAudioEncoding( file )) {
						case eAudioEncoding.MP3:
							SetMp3Favorite( args, track, file );
							break;

//						case eAudioEncoding.FLAC:
//						case eAudioEncoding.OGG:
//							SetVorbisFavorite( args, track, file );
//							break;

//						case eAudioEncoding.WMA:
//							SetWmaFavorite( args, track, file );
//							break;
					}
				}
			}
		}

		private void SetMp3Favorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mDataProvider.GetPhysicalFilePath( file );

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
						NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetFavorite - Queueing for later: {0}", track.Name ));

						lock( mUnfinishedCommands ) {
							mUnfinishedCommands.Add( args );
						}
					}
				}
			}
		}

/*		private void SetVorbisFavorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

			ClearReadOnlyFlag( filePath );

			var tags = File.Create( filePath );
			var flacTags = tags.GetTag( TagTypes.Xiph ) as TagLib.Ogg.XiphComment;

			if( flacTags != null ) {
				var	f = flacTags.GetFirstField( "" );
			}
		}
*/
/*		private void SetWmaFavorite( SetFavoriteCommandArgs args, DbTrack track, StorageFile file ) {
			var filePath = mNoiseManager.DataProvider.GetPhysicalFilePath( file );

			ClearReadOnlyFlag( filePath );

			var tags = File.Create( filePath );
			var wmaTags = tags.GetTag( TagTypes.Asf ) as TagLib.Asf.Tag;

			if( wmaTags != null ) {
				var d = wmaTags.GetDescriptorString( "" );
			}
		}
*/
		private void OnSetRating( SetRatingCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mDataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mDataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mDataProvider.GetPhysicalFilePath( file );

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
								NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetRating - Queueing for later: {0}", track.Name ));

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
			
			var item = mDataProvider.GetItem( args.ItemId );

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mDataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mDataProvider.GetPhysicalFilePath( file );

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
								NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:UpdatePlayCount - Queueing for later: {0}", track.Name ));

								lock( mUnfinishedCommands ) {
									mUnfinishedCommands.Add( args );
								}
							}
						}
					}
				}
			}
		}

		private void OnSetMp3Tags( SetMp3TagCommandArgs args ) {
			if( args.IsAlbum ) {
				NoiseLogger.Current.LogInfo( "Updating Mp3 file tags for album." );

				using( var trackList = mDataProvider.GetTrackList( args.ItemId )) {
					foreach( var track in trackList.List ) {
						SetMp3FileTags( new SetMp3TagCommandArgs( track, args ));
					}
				}
			}
			else {
				NoiseLogger.Current.LogInfo( "Updating Mp3 file tags for file." );

				SetMp3FileTags( args );
			}
		}

		private void SetMp3FileTags( SetMp3TagCommandArgs args ) {
			Condition.Requires( args ).IsNotNull();
			
			var item = mDataProvider.GetItem( args.ItemId );

			Condition.Requires( item ).IsNotNull();
			Condition.Requires( item ).IsOfType( typeof( DbTrack ));

			if(( item != null ) &&
			   ( item is DbTrack )) {
				var track = item as DbTrack;
				var file = mDataProvider.GetPhysicalFile( track );

				if( file != null ) {
					var filePath = mDataProvider.GetPhysicalFilePath( file );

					ClearReadOnlyFlag( filePath );

					var tags = File.Create( filePath );

					if( args.SetPublishedYear ) {
						tags.Tag.Year = args.PublishedYear;
					}
					if( args.SetName ) {
						tags.Tag.Title = args.Name;
					}

					try {
						tags.Save();
					}
					catch( Exception ) {
						NoiseLogger.Current.LogMessage( string.Format( "FileUpdates:SetMp3FileTags - Queueing for later: {0}", track.Name ));

						lock( mUnfinishedCommands ) {
							mUnfinishedCommands.Add( args );
						}
					}
				}
			}
		}

		private static void OnExecutionComplete( object sender, AsyncCommandCompleteEventArgs args ) {
			if(( args != null ) &&
			   ( args.Exception != null )) {
				NoiseLogger.Current.LogException( "Exception - FileUpdates:OnExecutionComplete:", args.Exception );
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
					NoiseLogger.Current.LogException( string.Format( "Exception FileUpdates:ClearReadOnlyFlag for file: ({0}) -", file ), ex );
				}
			}
		}
	}
}
