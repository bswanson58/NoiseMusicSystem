using System;

namespace MilkBottle.Types {
    class Error {
        public string       Message { get; }
        public Exception    Exception { get; }

        public Error( string message ) {
            Message = message;
        }

        public Error( string message, Exception ex ) :
            this( message ) {
            Exception = ex; 
        }

        public Error( Exception ex ) {
            Message = String.Empty;
            Exception = ex;
        }

        public static implicit operator Error( Exception ex ) {
            return new Error( String.Empty, ex );
        }
    }

    class DatabaseError : Error {
        public DatabaseError( string message ) : base( message ) { }
        public DatabaseError( Exception ex ) : base( ex ) { }
        public DatabaseError( string message, Exception ex ) : base( message, ex ) { }
    }
}
