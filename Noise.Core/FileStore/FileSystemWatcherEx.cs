using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Noise.Core.FileStore {
	public class FileSystemWatcherEx {
		private readonly Dictionary<string, DateTime>	mChangeEntries;
		private string				mRootPath;
		private FileSystemWatcher	mFileEvents;
		private Timer				mNotifyTimer;
		private bool				mIsWatching;
		private	Action<string>		mOnChangeAction;

		public	TimeSpan			NotifyDelay { get; set; }

		public FileSystemWatcherEx() {
			mChangeEntries = new Dictionary<string, DateTime>();
			NotifyDelay = new TimeSpan( 0, 0, 0, 10 );
		}

		public bool IsWatching {
			get{ return( mIsWatching ); }
		}

		public bool Initialize( string rootPath, Action<string> onChangeAction ) {
			mIsWatching = false;
			mOnChangeAction = onChangeAction;

			if( Directory.Exists( rootPath )) {
				mRootPath = rootPath;
				mFileEvents = new FileSystemWatcher( mRootPath ) { IncludeSubdirectories = true };

				mFileEvents.Changed += OnFileSystemEvent;
				mFileEvents.Created += OnFileSystemEvent;
				mFileEvents.Deleted += OnFileSystemEvent;
				mFileEvents.Renamed += OnFileSystemEvent;

				mFileEvents.EnableRaisingEvents = true;

				mNotifyTimer = new Timer( OnTimer );
				mNotifyTimer.Change( 50, 500 );

				mIsWatching = true;
			}

			return( mIsWatching );
		}

		public void Shutdown() {
			mFileEvents.EnableRaisingEvents = false;
			mNotifyTimer.Change( long.MaxValue, long.MaxValue );
			mChangeEntries.Clear();
		}

		private void OnFileSystemEvent( object sender, FileSystemEventArgs e ) {
			if( mChangeEntries.ContainsKey( e.FullPath )) {
				mChangeEntries[e.FullPath] = DateTime.Now;
			}
			else {
				mChangeEntries.Add( e.FullPath, DateTime.Now );
			}
		}

		private void OnTimer( object sender ) {
			if( mChangeEntries.Count > 0 ) {
				var triggerThresold = DateTime.Now - NotifyDelay;
				var triggerList = mChangeEntries.Keys.Where( path => mChangeEntries[path] < triggerThresold ).ToList();

				foreach( var path in triggerList ) {
					if( mChangeEntries.ContainsKey( path )) {
						mChangeEntries.Remove( path );
					}
				}

				if(( triggerList.Count > 0 ) &&
				   ( mChangeEntries.Count == 0 )) {
					FireFileEvent();
				}
			}
		}

		private void FireFileEvent() {
			if( mOnChangeAction != null ) {
				mOnChangeAction( mRootPath );
			}
		}
	}
}
