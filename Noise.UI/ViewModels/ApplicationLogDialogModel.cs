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
			var	retValue = mLogReader.ReadLog( Constants.ApplicationLogName );

			RaisePropertyChanged( () => LogText );

			return( retValue );
		}

		public string LogText {
			get{ return( mLogReader.LogText ); }
		}

		public void Execute_Update() {
			mLogReader.ReadLog( Constants.ApplicationLogName );

			RaisePropertyChanged( () => LogText );
		}
	}
}
