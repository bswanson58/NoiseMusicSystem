using System;
using System.Collections.Generic;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.Dto {
    public class VolumeItem : AutomaticCommandBase {
        private readonly Action<VolumeItem> mOnCollectVolume;
        private string                      mNameFormat;

        public  int                         VolumeIndex { get; }
        public  string                      Name => mNameFormat.Replace( "#", VolumeIndex.ToString());
        public  List<SourceItem>            VolumeList { get; }

        public VolumeItem( int index, Action<VolumeItem> onCollectVolume ) {
            VolumeIndex = index;
            mOnCollectVolume = onCollectVolume;

            VolumeList = new List<SourceItem>();
        }

        public void SetNameFormat( string format ) {
            mNameFormat = format;

            RaisePropertyChanged( () => Name );
        }

        public void Execute_CollectVolume() {
            mOnCollectVolume?.Invoke( this );
        }
    }
}
