using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Platform {
    class DriveEjector : IDriveEjector {
        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA( string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback );

        public Task OpenDrive( char driveLetter ) {
            return Task.Run( () => {
                if ( Char.IsLetter(driveLetter)) {
                    var returnString = String.Empty;

                    mciSendStringA( "open " + driveLetter + ": type CDaudio alias drive" + driveLetter, returnString, 0, 0 );
                    mciSendStringA( "set drive" + driveLetter + " door open", returnString, 0, 0 );
                }
            });
        }

        public Task CloseDrive( char driveLetter ) {
            return Task.Run(() => {
                if( Char.IsLetter( driveLetter )) {
                    var returnString = String.Empty;

                    mciSendStringA( "open " + driveLetter + ": type CDaudio alias drive" + driveLetter, returnString, 0, 0 );
                    mciSendStringA( "set drive" + driveLetter + " door closed", returnString, 0, 0 );
                }
            });
        }
    }
}
