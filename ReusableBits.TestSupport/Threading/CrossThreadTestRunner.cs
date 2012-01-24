using System;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;

namespace ReusableBits.TestSupport.Threading {
	public class CrossThreadTestRunner {
		private Exception mLastException;

		public void RunInMTA( ThreadStart userDelegate ) {
			Run( userDelegate, ApartmentState.MTA );
		}

		public void RunInSTA( ThreadStart userDelegate ) {
			Run( userDelegate, ApartmentState.STA );
		}

		private void Run( ThreadStart userDelegate, ApartmentState apartmentState ) {
			mLastException = null;

			var thread = new Thread(
			  delegate() {
				  try {
					  userDelegate.Invoke();
				  }
				  catch( Exception e ) {
					  mLastException = e;
				  }
			  } );
			thread.SetApartmentState( apartmentState );

			thread.Start();
			thread.Join();

			if( ExceptionWasThrown() )
				ThrowExceptionPreservingStack( mLastException );
		}

		private bool ExceptionWasThrown() {
			return mLastException != null;
		}

		[ReflectionPermission( SecurityAction.Demand )]
		private static void ThrowExceptionPreservingStack( Exception exception ) {
			FieldInfo remoteStackTraceString = typeof( Exception ).GetField( "_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic );

			if( remoteStackTraceString != null ) {
				remoteStackTraceString.SetValue( exception, exception.StackTrace + Environment.NewLine );
			}

			throw exception;
		}
	}
}
