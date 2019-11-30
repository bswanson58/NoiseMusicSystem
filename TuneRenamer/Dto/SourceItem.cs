﻿using System;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace TuneRenamer.Dto {
    public class SourceItem : AutomaticCommandBase {
        private bool                        mIsExpanded;

        public  string  Name { get; }
        public  string  FileName { get; }
        public  bool    IsSelectable {get; protected set; }

        protected SourceItem( string name, string filename ) {
            Name = name;
            FileName = filename;
        }

        public bool IsExpanded {
            get => mIsExpanded;
            set {
                mIsExpanded = value;

                RaisePropertyChanged( () => IsExpanded );
            }
        }
    }

    [DebuggerDisplay("SourceFile = {" + nameof( Name ) + "}")]
    public class SourceFile : SourceItem {
        private readonly Action<SourceFile> mInspectAction;
        private bool        mIsBeingRenamed;

        public  string      TagArtist { get; private set; }
        public  string      TagAlbum { get; private set; }
        public  int         TagIndex { get; private set; }
        public  string      TagTitle { get; private set; }
        public  string      TagName { get; private set; }
        public  string      ProposedName { get; private set; }
        public  bool        HasTagName => !String.IsNullOrWhiteSpace( TagName );
        public  bool        UseTagNameAsTarget { get; set; }
        public  bool        IsRenamable { get; }
        public  bool        IsInspectable { get; }
        public  bool        WillBeRenamed { get; private set; }

        public SourceFile( string fileName, bool isRenamable, bool isInspectable, Action<SourceFile> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName ) {
            IsRenamable = isRenamable;
            IsInspectable = isInspectable;
            mInspectAction = inspectAction;

            IsSelectable = false;
        }

        public void SetTags( string artist, string album, int index, string title, string name ) {
            TagArtist = artist;
            TagAlbum = album;
            TagIndex = index;
            TagTitle = title;
            TagName = name;

            RaisePropertyChanged( () => TagIndex );
            RaisePropertyChanged( () => TagTitle );
            RaisePropertyChanged( () => TagName );
            RaisePropertyChanged( () => HasTagName );
        }

        public void SetProposedName( string proposedName ) {
            ProposedName = proposedName;

            WillBeRenamed = !Name.Equals( ProposedName );
        }

        public bool IsBeingRenamed {
            get => mIsBeingRenamed;
            set {
                mIsBeingRenamed = value;

                RaisePropertyChanged( () => IsBeingRenamed );
            }
        }

        public void Execute_InspectItem() {
            mInspectAction?.Invoke( this );
        }
    }

    [DebuggerDisplay("SourceFolder = {" + nameof( Name ) + "}")]
    public class SourceFolder : SourceItem {
        private readonly Action<SourceFolder>   mCopyNames;
        private readonly Action<SourceFolder>   mCopyTags;

        public  BindableCollection<SourceItem>  Children { get; }

        public SourceFolder( string fileName, Action<SourceFolder> copyNames, Action<SourceFolder> copyTags ) :
            base( Path.GetFileName( fileName ), fileName ) {
            mCopyNames = copyNames;
            mCopyTags = copyTags;

            IsSelectable = true;
            Children = new BindableCollection<SourceItem>();
        }

        public void Execute_CopyNames() {
            mCopyNames?.Invoke( this );
        }

        public void Execute_CopyTags() {
            mCopyTags?.Invoke( this );
        }
    }
}
