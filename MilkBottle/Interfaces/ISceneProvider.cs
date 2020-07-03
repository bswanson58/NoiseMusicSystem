using System;
using System.Collections.Generic;
using LanguageExt;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface ISceneProvider {
        Either<Exception, Unit>                 SelectScenes( Action<IEnumerable<PresetScene>> action );

        Either<Exception, Unit>                 Insert( PresetScene scene );
        Either<Exception, Unit>                 Update( PresetScene scene );
        Either<Exception, Unit>                 Delete( PresetScene scene );
    }
}
