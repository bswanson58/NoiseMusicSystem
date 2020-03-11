using System;
using System.Collections.Generic;
using MilkBottle.Entities;

namespace MilkBottle.Dto {
    abstract class PresetList {
        public string   Name { get; }

        public abstract IEnumerable<Preset> GetPresets();

        protected PresetList( string name ) {
            Name = name;
        }
    }

    class LibraryPresetList : PresetList {
        private readonly PresetLibrary                              mLibrary;
        private readonly Func<PresetLibrary, IEnumerable<Preset>>   mRetrievalAction;

        public LibraryPresetList( PresetLibrary library, Func<PresetLibrary, IEnumerable<Preset>> retrieval ) :
            base( library.Name ) {
            mLibrary = library;
            mRetrievalAction = retrieval;
        }

        public override IEnumerable<Preset> GetPresets() {
            return mRetrievalAction?.Invoke( mLibrary );
        }
    }

    class SetPresetList : PresetList {
        private readonly PresetSet                              mSet;
        private readonly Func<PresetSet, IEnumerable<Preset>>   mRetrievalAction;

        public SetPresetList( PresetSet set, Func<PresetSet, IEnumerable<Preset>> retrieval ) :
            base( set.Name ) {
            mSet = set;
            mRetrievalAction = retrieval;
        }

        public override IEnumerable<Preset> GetPresets() {
            return mRetrievalAction?.Invoke( mSet );
        }
    }
}
