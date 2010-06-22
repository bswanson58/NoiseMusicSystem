using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Noise.Infrastructure.Support {
	/// <summary>
	/// Implements the INotifyPropertyChanged interface and 
	/// exposes a RaisePropertyChanged method for derived 
	/// classes to raise the PropertyChange event.  The event 
	/// arguments created by this class are cached to prevent 
	/// managed heap fragmentation.
	/// </summary>
	public abstract class BindableObject : INotifyPropertyChanged {
		private static readonly Dictionary<string, PropertyChangedEventArgs> mEventArgCache;
		private static readonly object mSyncRoot = new object();

		static BindableObject() {
			mEventArgCache = new Dictionary<string, PropertyChangedEventArgs>();
		}

		/// <summary>
		/// Raised when a public property of this object is set.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Returns an instance of PropertyChangedEventArgs for 
		/// the specified property name.
		/// </summary>
		/// <param name="propertyName">
		/// The name of the property to create event args for.
		/// </param>		
		private static PropertyChangedEventArgs GetPropertyChangedEventArgs( string propertyName ) {
			PropertyChangedEventArgs args;

			if( String.IsNullOrEmpty( propertyName )) {
				throw new ArgumentException( "propertyName cannot be null or empty." );
			}

			// Get the event args from the cache, creating them
			// and adding to the cache if necessary.
			lock( mSyncRoot ) {
				if(!mEventArgCache.TryGetValue( propertyName, out args )) {
					args = new PropertyChangedEventArgs( propertyName );
					mEventArgCache.Add( propertyName, args );
				}
			}

			return args;
		}

		/// <summary>
		/// Attempts to raise the PropertyChanged event, and 
		/// invokes the virtual AfterPropertyChanged method, 
		/// regardless of whether the event was raised or not.
		/// </summary>
		/// <param name="propertyName">
		/// The property which was changed.
		/// </param>
		private void RaisePropertyChanged( string propertyName ) {

			PropertyChangedEventHandler handler = PropertyChanged;
			if( handler != null ) {
				// Get the cached event args.
				PropertyChangedEventArgs args = GetPropertyChangedEventArgs( propertyName );

				// Raise the PropertyChanged event.
				handler( this, args );
			}
		}

		protected void NotifyOfPropertyChange<TProperty>( Expression<Func<TProperty>> property ) {
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;
			if( lambda.Body is UnaryExpression ) {
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else {
				memberExpression = (MemberExpression)lambda.Body;
			}

			RaisePropertyChanged( memberExpression.Member.Name );
		}

	}
}