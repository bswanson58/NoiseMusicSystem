using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Noise.UI.Adapters.DynamicProxies;
using Application = System.Windows.Application;

namespace Noise.UI.Support {
	public class DialogService : IDialogService {
		private readonly CompositionContainer	mContainer;

		public DialogService() {
			var catalog = new DirectoryCatalog( @".\" );
			mContainer = new CompositionContainer( catalog );
		}

		public bool? ShowDialog( string dialogName, object editObject ) {
			bool?	retValue = false;

			var dialogEnum = mContainer.GetExports<FrameworkElement>( dialogName );
			if( dialogEnum != null ) {
				var lazyDialog = ( from control in dialogEnum select control ).FirstOrDefault();

				if( lazyDialog != null ) {
					var dialogWindow = new DialogWindow();
					var dialogModel = new DialogWindowModel();
					var	dialogContent = lazyDialog.Value;
					var proxy = new DataErrorInfoProxy( editObject );

					dialogContent.DataContext = proxy;

					dialogModel.EditObject = proxy;
					dialogModel.DialogContent = dialogContent;

					proxy.BeginEdit();
					proxy.Validate();

					dialogWindow.DataContext = dialogModel;
					dialogWindow.Owner = Application.Current.MainWindow;

					retValue = dialogWindow.ShowDialog();
					if( retValue.GetValueOrDefault( false ) ) {
						proxy.EndEdit();
					}
					else {
						proxy.CancelEdit();
					}
				}
			}

			return ( retValue );
		}

		public bool? ShowDialog( string dialogName, object editObject, DialogModelBase viewModel ) {
			bool?	retValue = false;

			var dialogEnum = mContainer.GetExports<FrameworkElement>( dialogName );
			if( dialogEnum != null ) {
				var lazyDialog = ( from control in dialogEnum select control ).FirstOrDefault();

				if( lazyDialog != null ) {
					var dialogWindow = new DialogWindow();
					var dialogModel = new DialogWindowModel();
					var	dialogContent = lazyDialog.Value;
					var proxy = new DataErrorInfoProxy( editObject );

					viewModel.EditObject = proxy;
					dialogContent.DataContext = viewModel;

					dialogModel.EditObject = proxy;
					dialogModel.DialogContent = dialogContent;

					proxy.BeginEdit();
					proxy.Validate();

					dialogWindow.DataContext = dialogModel;
					dialogWindow.Owner = Application.Current.MainWindow;

					retValue = dialogWindow.ShowDialog();
					if( retValue.GetValueOrDefault( false ) ) {
						proxy.EndEdit();
					}
					else {
						proxy.CancelEdit();
					}
				}
			}

			return ( retValue );
		}

		public bool? ShowDialog( string dialogName, DialogModelBase viewModel ) {
			bool?	retValue = false;

			var dialogEnum = mContainer.GetExports<FrameworkElement>( dialogName );
			if( dialogEnum != null ) {
				var lazyDialog = ( from control in dialogEnum select control ).FirstOrDefault();

				if( lazyDialog != null ) {
					var dialogWindow = new DialogWindow();
					var dialogModel = new DialogWindowModel();
					var	dialogContent = lazyDialog.Value;

					dialogContent.DataContext = viewModel;
					dialogModel.DialogContent = dialogContent;
					dialogWindow.DataContext = dialogModel;
					dialogWindow.Owner = Application.Current.MainWindow;

					retValue = dialogWindow.ShowDialog();
				}
			}

			return ( retValue );
		}

		public bool? OpenFileDialog( string title, string extensions, string filter, out string fileName ) {
			fileName = "";

			var dlg = new Microsoft.Win32.OpenFileDialog { Title = title, DefaultExt = extensions, Filter = filter };
			var	retValue = dlg.ShowDialog();

			if( retValue.GetValueOrDefault( false ) ) {
				fileName = dlg.FileName;
			}

			return ( retValue );
		}

		public bool? SelectFolderDialog( string title, ref string path ) {
			bool?	retValue = false;

			var dialog = new FolderBrowserDialogEx {
				Description = title,
				ShowNewFolderButton = true,
				ShowEditBox = true,
				//NewStyle = false,
				SelectedPath = path,
				ShowFullPathInEditBox = false,
				RootFolder = System.Environment.SpecialFolder.MyComputer
			};

			if( dialog.ShowDialog() == DialogResult.OK ) {
				path = dialog.SelectedPath;
				retValue = true;
			}

			return( retValue );
		}
	}
}
