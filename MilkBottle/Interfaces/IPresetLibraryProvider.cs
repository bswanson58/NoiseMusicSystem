using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetLibraryProvider {
        Either<Exception, Option<PresetLibrary>>    GetLibraryById( ObjectId id );
        Either<Exception, Option<PresetLibrary>>    GetLibraryByName( string name );

        Either<Exception, Unit>                     QueryLibraries( Action<ILiteQueryable<PresetLibrary>> action );
        Either<Exception, Unit>                     SelectLibraries( Action<IEnumerable<PresetLibrary>> action );

        Either<Exception, Option<PresetLibrary>>    FindLibrary( BsonExpression predicate );
        Either<Exception, Unit>                     FindLibraryList( BsonExpression predicate, Action<IEnumerable<PresetLibrary>> action );

        Either<Exception, Unit>                     Insert( PresetLibrary library );
        Either<Exception, Unit>                     Update( PresetLibrary library );
        Either<Exception, Unit>                     Delete( PresetLibrary library );
    }
}
