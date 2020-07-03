
// from: https://stackoverflow.com/questions/2050396/getting-the-date-of-a-net-assembly
// original: https://blog.codinghorror.com/determining-build-date-the-hard-way/

namespace ReusableBits.Platform {
    public static class ApplicationInformation {
        private static System.Reflection.Assembly   mExecutingAssembly;
        private static System.Version               mExecutingAssemblyVersion;
        private static System.DateTime?             mCompileDate;

        /// <summary>
        /// Gets the executing assembly.
        /// </summary>
        /// <value>The executing assembly.</value>
        public static System.Reflection.Assembly ExecutingAssembly => mExecutingAssembly ?? (mExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary>
        /// Gets the executing assembly version.
        /// </summary>
        /// <value>The executing assembly version.</value>
        public static System.Version ExecutingAssemblyVersion => mExecutingAssemblyVersion ?? (mExecutingAssemblyVersion = ExecutingAssembly.GetName().Version);


        /// <summary>
        /// Gets the compile date of the currently executing assembly.
        /// </summary>
        /// <value>The compile date.</value>
        public static System.DateTime CompileDate {
            get {
                if(!mCompileDate.HasValue ) {
                    mCompileDate = RetrieveLinkerTimestamp( ExecutingAssembly.Location );
                }

                return mCompileDate ?? new System.DateTime();
            }
        }

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static System.DateTime RetrieveLinkerTimestamp( string filePath ) {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;

            try {
                s = new System.IO.FileStream( filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read );
                s.Read( b, 0, 2048 );
            }
            finally {
                s?.Close();
            }
            var dt = new System.DateTime( 1970, 1, 1, 0, 0, 0 ).AddSeconds( System.BitConverter.ToInt32( b, System.BitConverter.ToInt32(b, peHeaderOffset ) + linkerTimestampOffset ));

            return dt.AddHours( System.TimeZone.CurrentTimeZone.GetUtcOffset( dt ).Hours );
        }
    }
}
