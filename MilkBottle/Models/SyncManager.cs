using System.Collections.Generic;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using ReusableBits.Platform;

namespace MilkBottle.Models {
    class SyncManager : ISyncManager {
        private readonly ISceneProvider     mSceneProvider;

        public SyncManager( ISceneProvider sceneProvider ) {
            mSceneProvider = sceneProvider;
        }

        public PresetScene SelectScene( PlaybackEvent forEvent ) {
            var sceneList = new List<PresetScene>();

            mSceneProvider.SelectScenes( list => sceneList.AddRange( list ));

            return sceneList.FirstOrDefault();
        }
    }
}
