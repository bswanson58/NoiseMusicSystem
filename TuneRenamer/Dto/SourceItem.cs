using System;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace TuneRenamer.Dto {
    public class SourceItem : PropertyChangeBase {
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
        private bool            mIsBeingRenamed;
        private string          mProposedName;

        public  string          TagArtist { get; private set; }
        public  string          TagAlbum { get; private set; }
        public  int             TagIndex { get; private set; }
        public  string          TagTitle { get; private set; }
        public  string          TagName { get; private set; }
        public  bool            HasTagName => !String.IsNullOrWhiteSpace( TagName );
        public  bool            UseTagNameAsTarget { get; set; }
        public  bool            IsRenamable { get; }
        public  bool            IsInspectable { get; }
        public  bool            WillBeRenamed { get; private set; }

        public  DelegateCommand InspectItem { get; }

        public SourceFile( string fileName, bool isRenamable, bool isInspectable, Action<SourceFile> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName ) {
            InspectItem = new DelegateCommand( OnInspectItem );

            IsRenamable = isRenamable;
            IsInspectable = isInspectable;
            mInspectAction = inspectAction;

            IsSelectable = false;
            mProposedName = String.Empty;
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
            mProposedName = proposedName;

            WillBeRenamed = !Name.Equals( mProposedName );

            RaisePropertyChanged( () => ProposedName );
            RaisePropertyChanged( () => WillBeRenamed );
        }

        public void ClearProposedName() {
            mProposedName = String.Empty;

            WillBeRenamed = false;

            RaisePropertyChanged( () => ProposedName );
            RaisePropertyChanged( () => WillBeRenamed );
        }

        public string ProposedName {
            get {
                var retValue = mProposedName;

                if( String.IsNullOrWhiteSpace( retValue )) {
                    retValue = "Unnamed";
                }

                return retValue;
            }
        }

        public bool IsBeingRenamed {
            get => mIsBeingRenamed;
            set {
                mIsBeingRenamed = value;

                RaisePropertyChanged( () => IsBeingRenamed );
            }
        }

        private void OnInspectItem() {
            mInspectAction?.Invoke( this );
        }
    }

    [DebuggerDisplay("SourceFolder = {" + nameof( Name ) + "}")]
    public class SourceFolder : SourceItem {
        private readonly Action<SourceFolder>   mCopyNames;
        private readonly Action<SourceFolder>   mCopyTags;

        public  BindableCollection<SourceItem>  Children { get; }
        public  DelegateCommand                 CopyNames { get; }
        public  DelegateCommand                 CopyTags { get; }

        public SourceFolder( string fileName, Action<SourceFolder> copyNames, Action<SourceFolder> copyTags ) :
            base( Path.GetFileName( fileName ), fileName ) {
            mCopyNames = copyNames;
            mCopyTags = copyTags;

            CopyNames = new DelegateCommand( OnCopyNames );
            CopyTags = new DelegateCommand( OnCopyTags );

            IsSelectable = true;
            Children = new BindableCollection<SourceItem>();
        }

        private void OnCopyNames() {
            mCopyNames?.Invoke( this );
        }

        private void OnCopyTags() {
            mCopyTags?.Invoke( this );
        }
    }
}
