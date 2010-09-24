using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace Noise.UI.Adapters.DynamicProxies {
	public class EditableProxy : NotifyingProxy, IEditableObject {
		private class BackupState {
			private	Dictionary<string, object> OriginalValues { get; set; }
			public	Dictionary<string, object> NewValues { get; private set; }

			public BackupState() {
				OriginalValues = new Dictionary<string, object>();
				NewValues = new Dictionary<string, object>();
			}

			public void SetOriginalValue( string propertyName, object value ) {
				if(!OriginalValues.ContainsKey( propertyName )) {
					OriginalValues.Add( propertyName, value );
				}
			}

			public void SetNewValue( string propertyName, object value ) {
				if( OriginalValues.ContainsKey( propertyName ) && OriginalValues[propertyName] == value ) {
					return;
				}

				if( NewValues.ContainsKey( propertyName )) {
					NewValues[propertyName] = value;
				}
				else {
					NewValues.Add( propertyName, value );
				}
			}
		}

		private	BackupState mEditBackup;

		public EditableProxy() { }
		public EditableProxy( object proxiedObject ) : base( proxiedObject ) { }

		protected override void SetMember( string propertyName, object value ) {
			if( IsEditing ) {
				mEditBackup.SetOriginalValue( propertyName, GetPropertyInfo( propertyName ).GetValue( ProxiedObject, null ) );
				mEditBackup.SetNewValue( propertyName, value );
				RaisePropertyChanged( propertyName );
			}
			else {
				base.SetMember( propertyName, value );
			}
		}

		protected override object GetMember( string propertyName ) {
			return IsEditing && mEditBackup.NewValues.ContainsKey( propertyName ) ?
				mEditBackup.NewValues[propertyName] :
				base.GetMember( propertyName );
		}

		public override bool TryConvert( ConvertBinder binder, out object result ) {
			if( binder.Type == typeof( IEditableObject )) {
				result = this;
				return true;
			}

			return base.TryConvert( binder, out result );
		}

		public void BeginEdit() {
			if( !IsEditing ) {
				mEditBackup = new BackupState();
			}
		}

		public void CancelEdit() {
			if( IsEditing ) {
				mEditBackup = null;
			}
		}

		public void EndEdit() {
			if( IsEditing ) {
				var editObject = mEditBackup;
				mEditBackup = null;

				foreach( var item in editObject.NewValues ) {
					SetMember( item.Key, item.Value );
				}
			}
		}

		public bool IsEditing { get { return mEditBackup != null; } }
		public bool IsChanged { get { return IsEditing && mEditBackup.NewValues.Count > 0; } }
	}
}
