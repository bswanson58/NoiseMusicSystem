using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiteDB;
using MilkBottle.Entities;

namespace MilkBottle.Dto {
    enum PresetListType {
        Library = 1,
        Set = 2,
        Tag = 3,
        Preset = 4,
        Unrated = 5,
        DoNotPlay = 6,
        Duplicated = 7,
        AllPresets = 8
    }

    abstract class PresetList {
        public string   Name { get; }

        public abstract IEnumerable<Preset> GetPresets();
        public abstract PresetListType      ListType { get; }
        public abstract ObjectId            ListIdentifier { get; }

        protected PresetList( string name ) {
            Name = name;
        }
    }

    [DebuggerDisplay("List(Set): {" + nameof( Name ) + "}")]
    class LibraryPresetList : PresetList {
        private readonly PresetLibrary                              mLibrary;
        private readonly Func<PresetLibrary, IEnumerable<Preset>>   mRetrievalAction;

        public override PresetListType  ListType => PresetListType.Library;
        public override ObjectId        ListIdentifier => mLibrary?.Id;

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

        public override PresetListType  ListType => PresetListType.Set;
        public override ObjectId        ListIdentifier => mSet?.Id;

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

        public override PresetListType  ListType => PresetListType.Tag;
        public override ObjectId        ListIdentifier => mTag?.Id;

        public TagPresetList( PresetTag tag, Func<PresetTag, IEnumerable<Preset>> retrieval ) :
            base( tag.Name ) {
            mTag = tag;
            mRetrievalAction = retrieval;
        }

        public override IEnumerable<Preset> GetPresets() {
            return mRetrievalAction?.Invoke( mTag );
        }
    }

    class GlobalPresetList : PresetList {
        private readonly Func<IEnumerable<Preset>>   mRetrievalAction;

        public override PresetListType  ListType { get; }
        public override ObjectId        ListIdentifier => ObjectId.Empty;

        public GlobalPresetList( string name, PresetListType ofType, Func<IEnumerable<Preset>> retrieval ) :
            base( name ) {
            ListType = ofType;
            mRetrievalAction = retrieval;
        }

        public override IEnumerable<Preset> GetPresets() {
            return mRetrievalAction?.Invoke();
        }
    }
}
