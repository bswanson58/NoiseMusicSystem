using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IMoodProvider {
        Either<Exception, Option<Mood>>     GetMoodById( ObjectId id );

        Either<Exception, Unit>             SelectMoods( Action<IEnumerable<Mood>> action );

        Either<Exception, Unit>             Insert( Mood mood );
        Either<Exception, Unit>             Update( Mood mood );
        Either<Exception, Unit>             Delete( Mood mood );
    }
}
