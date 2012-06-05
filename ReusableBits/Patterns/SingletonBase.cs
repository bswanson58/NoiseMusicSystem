using System;

namespace ReusableBits.Patterns {
	/// <summary>
	/// A generic base class that may be used to implement a testable singleton.
	/// </summary>
	/// <typeparam name="TInterface">An interface exposed by the singleton</typeparam>
	/// <typeparam name="TClass">The concrete class implementing the interface</typeparam>
	public class SingletonBase<TInterface, TClass>
			where TInterface : class 
			where TClass : class, TInterface, new() {
		protected static Lazy<TInterface>		mDefault; 
		private   static volatile TInterface	mCurrent;
		private   static Func<TInterface>		mDefaultCreator;

		static SingletonBase() {
			mDefault = new Lazy<TInterface>( InternalCreateDefault );
		} 

		public static Func<TInterface> DefaultCreator {
			get { return( mDefaultCreator ); }
			set { mDefaultCreator = value; }
		}

		public static TInterface Current {
			get { return( mCurrent ?? ( mCurrent = mDefault.Value )); }
			set { mCurrent = value; }
		}

		protected static TInterface InternalCreateDefault() {
			if( mDefaultCreator != null ) {
				return( mDefaultCreator());
			}

			return( new TClass());
		}
	}
}
