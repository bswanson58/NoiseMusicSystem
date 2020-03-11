using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    enum QualifierField {
        IsFavorite,
        Rating,
        Name,
        Tags
    }

    enum QualifierOperation {
        Equal,
        NotEqual,
        Contains
    }

    [DebuggerDisplay("Qualifier: {" + nameof( Field ) + ":" + nameof( Operation ) + ":" + nameof( Value ) + "}")]
    class SetQualifier {
        public  QualifierField      Field { get; }
        public  QualifierOperation  Operation { get; }
        public  string              Value {  get; }

        public SetQualifier( QualifierField field, QualifierOperation operation, string value ) {
            Field = field;
            Operation = operation;
            Value = value;
        }
    }

    [DebuggerDisplay("Set: {" + nameof( Name ) + "}")]
    class PresetSet : EntityBase {
        public  string              Name { get; }
        public  List<SetQualifier>  Qualifiers { get; set; }

        public PresetSet( string name ) :
            base( ObjectId.NewObjectId()) {
            Name = name ?? String.Empty;

            Qualifiers = new List<SetQualifier>();
        }

        [BsonCtorAttribute]
        public PresetSet( ObjectId id, string name ) :
            base( id ) {
            Name = name ?? String.Empty;

            Qualifiers = new List<SetQualifier>();
        }
    }
}
