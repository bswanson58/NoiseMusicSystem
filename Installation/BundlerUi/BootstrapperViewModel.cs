using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BundlerUi.Support;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using ErrorEventArgs = Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ErrorEventArgs;

namespace BundlerUi {
	public class BootstrapperViewModel : PropertyChangedBase {
		private const string	cPackageNoise			= "NoisePackage";

		internal class InstallState {
			public const int	cUnknown				= 0;
			public const int	cPrerequisitesRequired	= 1;
			public const int	cPrerequisitesInstalled	= 2;
			public const int	cInstalled				= 3;
			public const int	cIsExecuting			= 4;
			public const int	cCompleted				= 5;
		};

		internal enum InstallOperation {
			Unknown,
			Install,
			Repair,
			Uninstall
		}

		private	readonly BootstrapperApplication	mBootstrapper;
		public	bool								InvokeShutdownOnComplete { get; set; }
		public	bool								InstallEnabled { get; private set; }
		public	bool								RepairEnabled { get; private set; }
		public	bool								UninstallEnabled { get; private set; }
		public	ObservableCollection<string>		PrerequisitesList { get; private set; }
		public	int									CurrentState { get; private set; }
		public	string								ExecuteMessage { get; private set; }
		public	string								ExecuteStatus { get; private set; }
		public	int									ExecuteProgress { get; private set; }
		public	bool								HaveUnknownProgress { get; private set; }
		public	string								ResultMessage { get; private set; }
		public	string								ResultStatus { get; private set; }
		public	string								ReleaseVersion { get; private set; }
		public	string								PackageName { get; private set; }

		private InstallOperation					mInstallOperation;
		private readonly Dictionary<string, string>	mPackageNames;

		public BootstrapperViewModel( BootstrapperApplication bootstrapper ) {
			mBootstrapper = bootstrapper;

			SetNotificationDispatcher( BootstrapperApp.BootstrapperDispatcher );

			PrerequisitesList = new ObservableCollection<string>();
			mPackageNames = new Dictionary<string, string>();
			mInstallOperation = InstallOperation.Unknown;

			SetInstallState( InstallState.cUnknown );
			SetInstallEnabled( false );
			SetRepairEnabled( false );
			SetUninstallEnabled( false );
		}

		public bool Initialize() {
			mBootstrapper.ApplyBegin += OnApplyBegin;
			mBootstrapper.ApplyComplete += OnApplyComplete;

			mBootstrapper.DetectBegin += OnDetectBegin;
			mBootstrapper.DetectPackageComplete += OnDetectPackageComplete;
			mBootstrapper.DetectComplete += OnDetectCompleted;

			mBootstrapper.PlanComplete += OnPlanComplete;

			mBootstrapper.ExecuteBegin += OnExecuteBegin;
			mBootstrapper.ExecutePackageBegin += OnExecutePackageBegin;
			mBootstrapper.ExecuteProgress += OnExecuteProgress;
			mBootstrapper.ExecutePackageComplete += OnExecutePackageComplete;
			mBootstrapper.ExecuteComplete += OnExecuteComplete;

			mBootstrapper.Error += OnError;

			//ReleaseVersion = Assembly.GetAssembly( GetType()).GetName().Version.ToString();
			ReleaseVersion = mBootstrapper.Engine.StringVariables["WixBundleVersion"];
			LoadPackageData();

			return ( true );
		}

		private void LoadPackageData() {
			string folder = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

			if( folder != null ) {
				var manifestNamespace = (XNamespace)"http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";
				var root = XElement.Load( Path.Combine( folder, "BootstrapperApplicationData.xml" ));

				var packages = from pack in root.Elements( manifestNamespace + "WixPackageProperties" ) select pack;
				foreach( var package in packages ) {
					var packageId = package.Attribute( "Package" );
					var name = package.Attribute( "DisplayName" );

					if(( packageId != null ) &&
					   (!string.IsNullOrWhiteSpace( packageId.Value )) &&
					   ( name != null ) &&
					   (!string.IsNullOrWhiteSpace( name.Value ))) {
						mPackageNames.Add( packageId.Value, name.Value );
					}
				}

				var bundleData = root.Elements( manifestNamespace + "WixBundleProperties" ).FirstOrDefault();
				if( bundleData != null ) {
					var displayName = bundleData.Attribute( "DisplayName" );

					if( displayName != null ) {
						PackageName = displayName.Value;
					}
				}
			}
		}

		public void StartDetect() {
			mBootstrapper.Engine.Detect();
		}

		private void StartInstall() {
			mInstallOperation = InstallOperation.Install;

			mBootstrapper.Engine.Plan( LaunchAction.Install );
		}

		private void StartRepair() {
			mInstallOperation = InstallOperation.Repair;

			mBootstrapper.Engine.Plan( LaunchAction.Repair );
		}

		public void StartUninstall() {
			mInstallOperation = InstallOperation.Uninstall;

			mBootstrapper.Engine.Plan( LaunchAction.Uninstall );
		}

		public void StartShutdown() {
			BootstrapperApp.BootstrapperDispatcher.InvokeShutdown();
		}

		// Package Detection phase
		private void OnDetectBegin( object sender, DetectBeginEventArgs args ) {
			SetInstallEnabled( !args.Installed );
			SetRepairEnabled( args.Installed );
			SetUninstallEnabled( args.Installed );
		}

		/// <summary>
		/// Method that gets invoked when the Bootstrapper DetectPackageComplete event is fired.
		/// Checks the PackageId and sets the installation scenario. The PackageId is the ID
		/// specified in one of the package elements (msipackage, exepackage, msppackage,
		/// msupackage) in the WiX bundle.
		/// </summary>
		private void OnDetectPackageComplete( object sender, DetectPackageCompleteEventArgs args ) {
			if( args.PackageId != cPackageNoise ) {
				if( args.State == PackageState.Absent ) {
					BootstrapperApp.BootstrapperDispatcher.Invoke( () => PrerequisitesList.Add( GetPackageDisplayName( args.PackageId )));
				}
			}
		}

		private void OnDetectCompleted( object sender, DetectCompleteEventArgs args ) {
			if( UninstallEnabled ) {
				SetInstallState( InstallState.cInstalled );
			}
			else {
				SetInstallState( PrerequisitesList.Any() ? InstallState.cPrerequisitesRequired : InstallState.cPrerequisitesInstalled );
			}
		}

		private void OnApplyBegin( object sender, ApplyBeginEventArgs args ) {
			SetExecuteProgress( 0 );
			SetInstallState( InstallState.cIsExecuting );
			SetInitialExecutionMessage();

			SetInstallEnabled( false );
			SetRepairEnabled( false );
			SetUninstallEnabled( false );
		}

		/// <summary>
		/// Method that gets invoked when the Bootstrapper ApplyComplete event is fired.
		/// This is called after a bundle installation has completed. Make sure we updated the view.
		/// </summary>
		private void OnApplyComplete( object sender, ApplyCompleteEventArgs args ) {
			SetInstallState( InstallState.cCompleted );

			if( InvokeShutdownOnComplete ) {
				mBootstrapper.Engine.Log( LogLevel.Verbose, "Automatically invoking shutdown for non-interactive install" );

				StartShutdown();
			}
		}

		// Execution phase
		private void OnExecuteBegin( object sender, ExecuteBeginEventArgs args ) {
			SetOperatingExecutionMessage();
		}

		private void OnExecutePackageBegin( object sender, ExecutePackageBeginEventArgs args ) {
			SetCurrentPackage( args.PackageId );
		}

		private void OnExecutePackageComplete( object sender, ExecutePackageCompleteEventArgs args ) {
			SetCurrentPackage( string.Empty );
		}

		private void OnExecuteProgress( object sender, ExecuteProgressEventArgs args ) {
			SetExecuteProgress( args.OverallPercentage );
		}

		private void OnExecuteComplete( object sender, ExecuteCompleteEventArgs args ) {
			SetExecuteProgress( 100 );
			SetCurrentPackage( string.Empty );

			SetResultMessage();
			SetExecuteResult( args.Status );
		}

		private void OnError( object sender, ErrorEventArgs args ) {
			SetErrorMessage( args.PackageId, args.ErrorMessage );
		}

		/// <summary>
		/// Method that gets invoked when the Bootstrapper PlanComplete event is fired.
		/// If the planning was successful, it instructs the Bootstrapper Engine to
		/// install the packages.
		/// </summary>
		private void OnPlanComplete( object sender, PlanCompleteEventArgs args ) {
			if( args.Status >= 0 ) {
				mBootstrapper.Engine.Apply( System.IntPtr.Zero );
			}
		}

		private RelayCommand mInstallCommand;
		public RelayCommand InstallCommand {
			get { return mInstallCommand ?? ( mInstallCommand = new RelayCommand( InstallExecute, () => InstallEnabled )); }
		}
		private void InstallExecute( object parameter ) {
			StartInstall();
		}

		private RelayCommand mRepairCommand;
		public RelayCommand RepairCommand {
			get { return mRepairCommand ?? ( mRepairCommand = new RelayCommand( RepairExecute, () => UninstallEnabled )); }
		}
		private void RepairExecute( object parameter ) {
			StartRepair();
		}

		private RelayCommand mUninstallCommand;
		public RelayCommand UninstallCommand {
			get { return mUninstallCommand ?? ( mUninstallCommand = new RelayCommand( UninstallExecute, () => UninstallEnabled )); }
		}
		private void UninstallExecute( object parameter ) {
			StartUninstall();
		}

		private RelayCommand mExitCommand;
		public RelayCommand ExitCommand {
			get { return mExitCommand ?? ( mExitCommand = new RelayCommand( ExitExecute, () => CurrentState != InstallState.cIsExecuting )); }
		}
		private void ExitExecute( object parameter ) {
			BootstrapperApp.BootstrapperDispatcher.InvokeShutdown();
		}

		private void SetInstallEnabled( bool value ) {
			InstallEnabled = value;

			RaisePropertyChanged( () => InstallEnabled );
			BootstrapperApp.BootstrapperDispatcher.Invoke( () => InstallCommand.OnCanExecuteChanged());
		}

		private void SetRepairEnabled( bool value ) {
			RepairEnabled = value;

			RaisePropertyChanged( () => RepairEnabled );
			BootstrapperApp.BootstrapperDispatcher.Invoke( () => RepairCommand.OnCanExecuteChanged());
		}

		private void SetUninstallEnabled( bool value ) {
			UninstallEnabled = value;

			RaisePropertyChanged( () => UninstallEnabled );
			BootstrapperApp.BootstrapperDispatcher.Invoke( () => RepairCommand.OnCanExecuteChanged());
			BootstrapperApp.BootstrapperDispatcher.Invoke( () => UninstallCommand.OnCanExecuteChanged());
		}

		private void SetInstallState( int state ) {
			CurrentState = state;

			RaisePropertyChanged( () => CurrentState );
			BootstrapperApp.BootstrapperDispatcher.Invoke( () => ExitCommand.OnCanExecuteChanged());
		}

		private void SetCurrentPackage( string packageId ) {
			ExecuteStatus = !string.IsNullOrWhiteSpace( packageId ) ? string.Format( "Installing package: {0}", GetPackageDisplayName( packageId )) : string.Empty;

			RaisePropertyChanged( () => ExecuteStatus );
		}

		private void SetErrorMessage( string packageId, string message ) {
			ExecuteStatus = string.Format( "Package {0} has an error: {1}", GetPackageDisplayName( packageId ), message );

			RaisePropertyChanged( () => ExecuteStatus );
		}

		private void SetInitialExecutionMessage() {
			switch( mInstallOperation ) {
				case InstallOperation.Install:
					SetExecuteMessage( "Preparing for installation." );
					break;

				case InstallOperation.Repair:
					SetExecuteMessage( "Preparing to repair the installation" );
					break;

				case InstallOperation.Uninstall:
					SetExecuteMessage( "Preparing to uninstall." );
					break;
			}
		}

		private void SetOperatingExecutionMessage() {
			switch( mInstallOperation ) {
				case InstallOperation.Install:
					SetExecuteMessage( string.Format( "Installing {0}.", PackageName ));
					break;

				case InstallOperation.Repair:
					SetExecuteMessage( string.Format( "Repairing {0}.", PackageName ));
					break;

				case InstallOperation.Uninstall:
					SetExecuteMessage( string.Format( "Uninstalling {0}.", PackageName ));
					break;
			}
		}

		private void SetExecuteMessage( string message ) {
			ExecuteMessage = message;

			RaisePropertyChanged( () => ExecuteMessage );
		}

		private void SetExecuteProgress( int value ) {
			ExecuteProgress = value;

			SetHaveExecuteProgress( ExecuteProgress == 0 );

			RaisePropertyChanged( () => ExecuteProgress );
		}

		private void SetHaveExecuteProgress( bool value ) {
			if( value != HaveUnknownProgress ) {
				HaveUnknownProgress = value;

				RaisePropertyChanged( () => HaveUnknownProgress );
			}
		}

		private string GetPackageDisplayName( string packageId ) {
			var retValue = packageId;

			if( mPackageNames.ContainsKey( packageId )) {
				retValue = mPackageNames[packageId];
			}

			return( retValue );
		}

		private void SetResultMessage() {
			switch( mInstallOperation ) {
				case InstallOperation.Install:
					ResultMessage = "The installation has completed.";
					break;

				case InstallOperation.Repair:
					ResultMessage = "The repair operation has completed.";
					break;

				case InstallOperation.Uninstall:
					ResultMessage = "The uninstall operation has completed.";
					break;
			}

			RaisePropertyChanged( () => ResultMessage );
		}

		private void SetExecuteResult( int result ) {
			switch( result ) {
				case 0:
					ResultStatus = "successful";
					break;

				default:
					ResultStatus = "unknown";
					break;
			}

			RaisePropertyChanged( () => ResultStatus );
		}
	}
}
