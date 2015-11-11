using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class DetachDatabaseViewModel : AutomaticCommandBase,
										   IHandle<Events.SystemInitialized> {
		private readonly IEventAggregator					mEventAggregator;
		private readonly IDatabaseUtility					mDatabaseUtility;
		private readonly BindableCollection<DatabaseInfo>	mAvailableDatabases;
		private DatabaseInfo								mCurrentDatabase;

		public DetachDatabaseViewModel( IEventAggregator eventAggregator, IDatabaseUtility databaseUtility ) {
			mEventAggregator = eventAggregator;
			mDatabaseUtility = databaseUtility;

			mAvailableDatabases = new BindableCollection<DatabaseInfo>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized args ) {
			LoadDatabases();
		}

		public BindableCollection<DatabaseInfo> DatabaseList {
			get {  return( mAvailableDatabases ); }
		}

		public DatabaseInfo CurrentDatabase {
			get {  return( mCurrentDatabase ); }
			set {
				mCurrentDatabase = value;

				RaisePropertyChanged( () => CurrentDatabase );
			}
		}

		private void LoadDatabases() {
			mAvailableDatabases.Clear();
			mAvailableDatabases.AddRange( mDatabaseUtility.GetDatabaseList());

			CanEdit = mAvailableDatabases.Any();
		}

		public void Execute_DetachDatabase() {
			if( mCurrentDatabase != null ) {
				try {
					mDatabaseUtility.DetachDatabase( mCurrentDatabase );

					ProgressText = string.Format( "Database '{0}' was successfully detached.", mCurrentDatabase.PhysicalName );
				}
				catch( Exception ex ) {
					ProgressText = string.Format( "Failure detaching database '{0}'.", mCurrentDatabase.PhysicalName );
				}

				LoadDatabases();
			}
		}

		[DependsUpon( "CurrentDatabase" )]
		public bool CanExecute_DetachDatabase() {
			return( mCurrentDatabase != null );
		}

		public bool CanEdit {
			get {  return( Get( () => CanEdit )); }
			set {  Set( () => CanEdit, value ); }
		}

		public string ProgressText {
			get {  return( Get( () => ProgressText )); }
			set {  Set( () => ProgressText, value ); }
		}
	}
}
