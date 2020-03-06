using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetProvider : IDisposable {
        Either<Exception, Option<Preset>>   GetPresetById( ObjectId id );
        Either<Exception, Option<Preset>>   GetPresetByName( string name );

        Either<Exception, Unit>             QueryPresets( Action<ILiteQueryable<Preset>> action );
        Either<Exception, Unit>             SelectPresets( Action<IEnumerable<Preset>> action );

        Either<Exception, Option<Preset>>   FindPreset( BsonExpression expression );
        Either<Exception, Unit>             FindPresetList( BsonExpression expression, Action<IEnumerable<Preset>> action );

        Either<Exception, Unit>             Insert( Preset preset );
        Either<Exception, Unit>             Update( Preset preset );
        Either<Exception, Unit>             Delete( Preset preset );
    }
}
