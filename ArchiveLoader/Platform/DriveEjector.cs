using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Platform {
    class DriveEjector : IDriveEjector {
        [DllImport( "winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi )]
        protected static extern int mciSendStringA( string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback );

        public Task OpenDrive( string driveRoot ) {
            return Task.Run( () => {
                if((!String.IsNullOrWhiteSpace( driveRoot ) &&
                   ( driveRoot.Length > 1 ))) {
                    var returnCode = new StringBuilder( 80 );
                    var commandLine = "set " + driveRoot.Substring( 0, 2 ) + @"\*.cda door open";

                    mciSendStringA( commandLine, returnCode, returnCode.Length, new IntPtr( 0 ));
                }
            });
        }

        public Task CloseDrive( string driveRoot ) {
            return Task.Run( () => {
                if((!String.IsNullOrWhiteSpace( driveRoot ) &&
                   ( driveRoot.Length > 1 ))) {
                    var returnCode = new StringBuilder( 80 );
                    var commandLine = "set " + driveRoot.Substring( 0, 2 ) + @"\*.cda door closed";

                    mciSendStringA( commandLine, returnCode, returnCode.Length, new IntPtr( 0 ));
                }
            });
        }
    }
}
