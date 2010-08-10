using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class UserSettingsNotifier : BindableObject {
		private readonly IUserSettings	mTargetObject;

		public UserSettingsNotifier( IUserSettings targetObject ) {
			mTargetObject = targetObject;
		}

		public IUserSettings TargetItem {
			get{ return( mTargetObject ); }
		}

		public string Genre {
			get{ return( mTargetObject.Genre ); }
			set {
				mTargetObject.Genre = value;

				RaisePropertyChanged( () => Genre );
			}
		}

		public bool IsFavorite {
			get{ return( mTargetObject.IsFavorite ); }
			set {
				mTargetObject.IsFavorite = value;

				RaisePropertyChanged( () => IsFavorite );
			}
		}

		public Int16 Rating {
			get{ return( mTargetObject.Rating ); }
			set {
				mTargetObject.Rating = value;

				RaisePropertyChanged( () => Rating );
			}
		}
	}
}
