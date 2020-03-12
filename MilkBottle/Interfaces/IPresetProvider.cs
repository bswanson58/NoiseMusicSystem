using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetProvider {
        Either<Exception, Option<Preset>>   GetPresetById( ObjectId id );
        Either<Exception, Option<Preset>>   GetPresetByName( string name );

        Either<Exception, Unit>             SelectPresets( Action<IEnumerable<Preset>> action );
        Either<Exception, Unit>             SelectPresets( PresetLibrary forLibrary, Action<IEnumerable<Preset>> action );

        Either<Exception, Option<Preset>>   FindPreset( string expression );
        Either<Exception, Unit>             FindPresetList( string expression, Action<IEnumerable<Preset>> action );

        Either<Exception, Unit>             Insert( Preset preset );
        Either<Exception, Unit>             Update( Preset preset );
        Either<Exception, Unit>             Delete( Preset preset );
    }
}
