using System;
using System.Runtime.InteropServices;

namespace Noise.Core.Platform {
	public class SleepPreventer : IDisposable {
        // Import SetThreadExecutionState Win32 API and necessary flags
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState( uint esFlags );
        public const uint ES_CONTINUOUS			= 0x80000000;
        public const uint ES_SYSTEM_REQUIRED	= 0x00000001;

        private	readonly uint	mPreviousState;

		public SleepPreventer() {
            mPreviousState = SetThreadExecutionState( ES_CONTINUOUS | ES_SYSTEM_REQUIRED );

            if( 0 == mPreviousState ) {
				throw new ApplicationException( "SleepPreventer:SetThreadExecutionState could not be set" );
            }
		}

		public void Dispose() {
            SetThreadExecutionState( mPreviousState );
		}
	}
}
