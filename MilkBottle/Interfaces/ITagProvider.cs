using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface ITagProvider {
        Either<Exception, Option<PresetTag>>    GetTagById( ObjectId id );
        Either<Exception, Option<PresetTag>>    GetTagByName( string name );

        Either<Exception, Unit>                 SelectTags( Action<IEnumerable<PresetTag>> action );

        Either<Exception, Unit>                 Insert( PresetTag tag );
        Either<Exception, Unit>                 Update( PresetTag tag );
        Either<Exception, Unit>                 Delete( PresetTag tag );
    }
}
