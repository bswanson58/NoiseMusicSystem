using System.IO;
using ForRent.Interfaces;
using Newtonsoft.Json;

namespace ForRent.Platform {
    public class JsonObjectWriter : IFileWriter {
        public void Write<T>( string path, T item ) {
            if(!Equals( item, default( T ))) {
                var	json = JsonConvert.SerializeObject( item, Formatting.Indented );

                using( var file = File.CreateText( path )) {
                    file.Write( json );
                    file.Close();
                }
            }
        }

        public T Read<T>( string path ) {
            var retValue = default( T );

            if( File.Exists( path )) {
                using( var file = File.OpenText( path )) {
                    retValue = JsonConvert.DeserializeObject<T>( file.ReadToEnd());
                }
            }

            return( retValue );
        }
    }
}
