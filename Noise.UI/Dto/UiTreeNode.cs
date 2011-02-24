using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiTreeNode : ViewModelBase {
		public bool IsSelected {
			get { return( Get( () => IsSelected )); }
			set {
				Set( () => IsSelected, value );

				Onselect();
			}
		}

		public bool IsExpanded {
			get { return( Get( () => IsExpanded )); }
			set {
				if( value ) {
 					IsSelected = true;
				}

				Set( () => IsExpanded, value  );

				OnExpand();
			}
		}

		protected virtual void Onselect() { }
		protected virtual void OnExpand() { }

		public virtual string IndexString {
			get{ return( "" ); }
		}
	}
}
