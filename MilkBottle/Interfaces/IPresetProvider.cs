using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Types;

namespace MilkBottle.Interfaces {
    interface IPresetProvider {
        Either<DatabaseError, Preset>   GetPresetById( ObjectId id );
        Either<DatabaseError, Preset>   GetPresetByName( string name );

        Either<DatabaseError, Unit>     QueryPresets( Action<ILiteQueryable<Preset>> action );
        Either<DatabaseError, Unit>     SelectPresets( Action<IEnumerable<Preset>> action );

        Either<DatabaseError, Preset>   FindPreset( BsonExpression predicate );
        Either<DatabaseError, Unit>     FindPresetList( BsonExpression predicate, Action<IEnumerable<Preset>> action );

        Either<DatabaseError, Unit>     Insert( Preset preset );
        Either<DatabaseError, Unit>     Update( Preset preset );
        Either<DatabaseError, Unit>     Delete( Preset preset );
    }
}
