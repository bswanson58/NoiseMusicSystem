using System;

namespace MilkBottle.Types {
    class ValidationException : ApplicationException {
        public ValidationException( string message ) :
            base( message ) { }
    }
}
