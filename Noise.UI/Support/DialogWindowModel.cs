using System.ComponentModel;
using System.Windows;
using Noise.UI.Adapters.DynamicProxies;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Support {
    public interface IDialogWindow {
        void    Close( bool isConfirmed );
    }

	internal class DialogWindowModel : AutomaticPropertyBase, IDialogWindow {
		public FrameworkElement			DialogContent { get; set; }
		private bool?					mDialogResult;
		private INotifyPropertyChanged	mEditObjectChanged;

		public bool? DialogResult {
			get => ( mDialogResult );
		    set{ 
				mDialogResult = value; 

				RaisePropertyChanged( () => DialogResult );
			}
		}

		public object EditObject {
			get{ return( Get( () => EditObject )); }
			set{
				Set( () => EditObject, value );

				if( mEditObjectChanged != null ) {
					mEditObjectChanged.PropertyChanged -= EditObjectPropertyChanged;
				}
				mEditObjectChanged = EditObject as INotifyPropertyChanged;
				if( mEditObjectChanged != null ) {
					mEditObjectChanged.PropertyChanged += EditObjectPropertyChanged;
				}
			}
		}

		private int EditObjectChanged {
			get{ return( Get( () => EditObjectChanged )); }
			set{ Set( () => EditObjectChanged, value ); }
		}

		private void EditObjectPropertyChanged( object sender, PropertyChangedEventArgs e ) {
			EditObjectChanged++;
		}

		public void Execute_Ok( object sender ) {
			var proxy = EditObject as DataErrorInfoProxy;

			if( proxy != null ) {
				proxy.Validate();

				if( proxy.HasErrors ) {
					MessageBox.Show( proxy.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
				else {
					DialogResult = true;
				}
			}
			else {
				DialogResult = true;
			}
		}

		[DependsUpon( "EditObjectChanged" )]
		public bool CanExecute_Ok( object sender ) {
			var retValue = false;

			var proxy = EditObject as DataErrorInfoProxy;

			if( proxy != null ) {
				proxy.Validate();

				if(!proxy.HasErrors ) {
					retValue = true;
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}

		public void Execute_Cancel( object sender ) {
			DialogResult = false;
		}

	    public void Close( bool isConfirmed ) {
            DialogResult = isConfirmed;
	    }
	}
}
