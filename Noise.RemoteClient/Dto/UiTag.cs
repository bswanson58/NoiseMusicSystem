using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.Dto {
    class UiTag : BindableBase {
        private readonly TagInfo    mTag;
        private bool                mIsTagged;

        public  long                TagId => mTag.TagId;
        public  string              TagName => mTag.TagName;
        
        public UiTag( TagInfo tag ) {
            mTag = tag;

            IsTagged = false;
        }

        public bool IsTagged {
            get => mIsTagged;
            set => SetProperty( ref mIsTagged, value );
        }
    }
}
