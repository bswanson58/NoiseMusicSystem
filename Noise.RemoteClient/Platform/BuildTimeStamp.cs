using System;
using System.IO;
using System.Reflection;

namespace Noise.RemoteClient.Platform {
    public static class BuildTimeStamp {
        public static DateTime GetTimestamp() {
            var retValue = DateTime.MinValue;

            try {
                var assembly = Assembly.GetExecutingAssembly();

                using( var stream = assembly.GetManifestResourceStream( "Noise.RemoteClient.Resources.BuildTimeStamp.txt" )) {
                    if( stream != null ) {
                        using( var reader = new StreamReader( stream ) ) {
                            var time = reader.ReadToEnd();

                            retValue = DateTime.Parse( time );
                        }
                    }
                }
            }
            catch( Exception ) {
                retValue = DateTime.MinValue;
            }

            return retValue;
        }
    }
}
