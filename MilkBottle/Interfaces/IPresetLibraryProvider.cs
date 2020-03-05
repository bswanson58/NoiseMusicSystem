using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Types;

namespace MilkBottle.Interfaces {
    interface IPresetLibraryProvider {
        Either<DatabaseError, PresetLibrary>    GetLibraryById( ObjectId id );
        Either<DatabaseError, PresetLibrary>    GetLibraryByName( string name );

        Either<DatabaseError, Unit>             QueryLibraries( Action<ILiteQueryable<PresetLibrary>> action );
        Either<DatabaseError, Unit>             SelectLibraries( Action<IEnumerable<PresetLibrary>> action );

        Either<DatabaseError, PresetLibrary>    FindLibrary( BsonExpression predicate );
        Either<DatabaseError, Unit>             FindLibraryList( BsonExpression predicate, Action<IEnumerable<PresetLibrary>> action );

        Either<DatabaseError, Unit>             Insert( PresetLibrary library );
        Either<DatabaseError, Unit>             Update( PresetLibrary library );
        Either<DatabaseError, Unit>             Delete( PresetLibrary library );
    }
}
