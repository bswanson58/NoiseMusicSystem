using System;
using System.ComponentModel;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class UserSettingsNotifier : BindableObject {
		private readonly IUserSettings	mTargetObject;
		public	long					UiGenre { get; private set; }
		public	bool					UiIsFavorite { get; private set; }
		public	Int16					UiRating { get; private set; }

		public UserSettingsNotifier( IUserSettings targetObject, INotifyPropertyChanged changer ) {
			mTargetObject = targetObject;

			if( changer != null ) {
				changer.PropertyChanged += OnPropertyChanged;
			}
		}

		private void OnPropertyChanged( object sender, PropertyChangedEventArgs args ) {
			RaisePropertyChanged( args.PropertyName );
		}


		public IUserSettings TargetItem {
			get{ return( mTargetObject ); }
		}

		public long Genre {
			get{ return( mTargetObject.Genre ); }
			set {
				UiGenre = value;

				RaisePropertyChanged( () => UiGenre );
			}
		}

		public bool IsFavorite {
			get{ return( mTargetObject.IsFavorite ); }
			set {
				UiIsFavorite = value;

				RaisePropertyChanged( () => UiIsFavorite );
			}
		}

		public Int16 Rating {
			get{ return( mTargetObject.Rating ); }
			set {
				UiRating = value;

				RaisePropertyChanged( () => UiRating );
			}
		}
	}
}
