using System;
using System.Windows.Input;

namespace BundlerUi.Support {
	public class RelayCommand : ICommand {
		private readonly Action<object>	mExecute;
		private readonly Func<bool>		mCanExecute;

		public event EventHandler		CanExecuteChanged = delegate { };

		public RelayCommand( Action<object> execute )
			: this( execute, null ) { }

		public RelayCommand( Action<object> execute, Func<bool> canExecute ) {
			mExecute = execute;
			mCanExecute = canExecute;
		}


		public bool CanExecute( object parameter ) {
			return(( mCanExecute == null ) ||
				   ( mCanExecute()));
		}

		public void Execute( object parameter ) {
			if( mExecute != null )
				mExecute( parameter );
		}

		public void OnCanExecuteChanged() {
			CanExecuteChanged( this, EventArgs.Empty );
		}
	}
}
