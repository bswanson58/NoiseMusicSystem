using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Noise.Infrastructure.Support {
	public class ObservableCollectionEx<T> : ObservableCollection<T> {
		private bool mSuppressNotification;
		private bool mNotificationPending;

		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e ) {
			if( !mSuppressNotification ) {
				base.OnCollectionChanged( e );
			}

			mNotificationPending = true;
		}

		public void SuspendNotification() {
			mSuppressNotification = true;
		}

		public void ResumeNotification() {
			mSuppressNotification = false;

			if( mNotificationPending ) {
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ));
				mNotificationPending = false;
			}
		}

		public void AddRange( IEnumerable<T> list ) {
			if( list == null ) {
				throw new ArgumentNullException( "list" );
			}

			SuspendNotification();

			foreach( T item in list ) {
				Add( item );
			}

			ResumeNotification();
		}
	}
}
