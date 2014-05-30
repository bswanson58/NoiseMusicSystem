using Caliburn.Micro;
using Noise.Librarian.Models;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ShellViewModel : AutomaticPropertyBase, IHandle<ProgressEvent> {
		private const string				cProgressNormal = "Normal";
		private const string				cProgressNone = "None";

		public ShellViewModel( IEventAggregator eventAggregator ) {
			ProgressState = cProgressNone;

			eventAggregator.Subscribe( this );
		}

		public void Handle( ProgressEvent args ) {
			ProgressState = args.IsActive ? cProgressNormal : cProgressNone;
			Progress = args.Progress;
		}

		public double Progress {
			get {  return( Get( () => Progress )); }
			set {  Set( () => Progress, value ); }
		}

		public string ProgressState {
			get {  return( Get( () => ProgressState )); }
			set {  Set( () => ProgressState, value ); }
		}
	}
}
