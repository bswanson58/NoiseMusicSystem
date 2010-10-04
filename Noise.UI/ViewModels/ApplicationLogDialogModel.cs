using System;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ApplicationLogDialogModel : DialogModelBase {
		private readonly ApplicationLogReader	mLogReader;

		public ApplicationLogDialogModel() {
			mLogReader = new ApplicationLogReader();
		}

		public bool Initialize() {
			return( ReadLog());
		}

		public string LogText {
			get{ return( mLogReader.LogText ); }
		}

		public void Execute_Update() {
			ReadLog();
		}

		private bool ReadLog() {
			var logPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );
			var	retValue = mLogReader.ReadLog( Path.Combine( logPath, Constants.ApplicationLogName ));

			RaisePropertyChanged( () => LogText );

			return( retValue );
		}
	}
}
