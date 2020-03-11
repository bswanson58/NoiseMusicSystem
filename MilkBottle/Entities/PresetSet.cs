using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteDB;

namespace MilkBottle.Entities {
    enum QualifierField {
        IsFavorite = 1,
        Rating = 2,
        Name = 3,
        Tags = 4,
        Unknown = 0
    }

    enum QualifierOperation {
        Equal = 1,
        NotEqual = 2,
        Contains = 3
    }

    [DebuggerDisplay("{" + nameof( DebugDisplay ) + "}")]
    class SetQualifier {
        public  QualifierField      Field { get; }
        public  QualifierOperation  Operation { get; }
        public  string              Value { get; }

        public  string              DebugDisplay => $"Qualifier: {Field}-{Operation}-{Value}";

        [BsonCtorAttribute]
        public SetQualifier( int field, int operation, string value ) {
            Field = (QualifierField)field;
            Operation = (QualifierOperation)operation;
            Value = value;
        }

        public SetQualifier( QualifierField field, QualifierOperation operation, string value ) {
            Field = field;
            Operation = operation;
            Value = value;
        }

        public BsonValue TypedValue() {
            BsonValue retValue = Value;

            switch( Field ) {
                case QualifierField.IsFavorite:
                    retValue = Boolean.Parse( Value );
                    break;

                case QualifierField.Rating:
                    retValue = Int16.Parse( Value );
                    break;
            }

            return retValue;
        }
    }

    [DebuggerDisplay("Set: {" + nameof( Name ) + "}")]
    class PresetSet : EntityBase {
        public  string              Name { get; }
        public  List<SetQualifier>  Qualifiers { get; set; }

        public PresetSet( string name ) :
            base( ObjectId.NewObjectId()) {
            Name = ( name ?? String.Empty ).Trim();

            Qualifiers = new List<SetQualifier>();
        }

        [BsonCtorAttribute]
        public PresetSet( ObjectId id, string name ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();

            Qualifiers = new List<SetQualifier>();
        }

        public PresetSet WithQualifier( SetQualifier qualifier ) {
            var retValue = WithoutQualifier( qualifier.Field );

            retValue.Qualifiers.Add( qualifier );

            return retValue;
        }

        public PresetSet WithoutQualifier( QualifierField field ) {
            var qualifier = Qualifiers.FirstOrDefault( q => q.Field.Equals( field ));

            if( qualifier != null ) {
                Qualifiers.Remove( qualifier );
            }

            return this;
        }
    }
}
