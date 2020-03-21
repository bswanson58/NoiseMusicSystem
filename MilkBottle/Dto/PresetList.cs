using System;
using System.Collections.Generic;
using System.Diagnostics;
using MilkBottle.Entities;

namespace MilkBottle.Dto {
    abstract class PresetList {
        public string   Name { get; }

        public abstract IEnumerable<Preset> GetPresets();

        protected PresetList( string name ) {
            Name = name;
        }
    }

    [DebuggerDisplay("List(Set): {" + nameof( Name ) + "}")]
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

    [DebuggerDisplay("List(Library): {" + nameof( Name ) + "}")]
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

    [DebuggerDisplay("List(Tag): {" + nameof( Name ) + "}")]
    class TagPresetList : PresetList {
        private readonly PresetTag  mTag;
        private readonly Func<PresetTag, IEnumerable<Preset>>   mRetrievalAction;

        public TagPresetList( PresetTag tag, Func<PresetTag, IEnumerable<Preset>> retrieval ) :
            base( tag.Name ) {
            mTag = tag;
            mRetrievalAction = retrieval;
        }

        public override IEnumerable<Preset> GetPresets() {
            return mRetrievalAction?.Invoke( mTag );
        }
    }
}
