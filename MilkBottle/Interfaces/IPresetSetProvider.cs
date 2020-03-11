using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetSetProvider {
        Either<Exception, Option<PresetSet>>    GetSetById( ObjectId id );
        Either<Exception, Option<PresetSet>>    GetSetByName( string name );

        Either<Exception, Unit>                 SelectSets( Action<IEnumerable<PresetSet>> action );

        Either<Exception, Option<PresetSet>>    FindSet( BsonExpression predicate );
        Either<Exception, Unit>                 FindSetList( BsonExpression predicate, Action<IEnumerable<PresetSet>> action );

        Either<Exception, Unit>                 Insert( PresetSet set );
        Either<Exception, Unit>                 Update( PresetSet set );
        Either<Exception, Unit>                 Delete( PresetSet set );
    }
}
