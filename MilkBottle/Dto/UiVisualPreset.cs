using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using MilkBottle.Entities;

namespace MilkBottle.Dto {
    class UiPresetCategory {
        public  string  CategoryName { get; }
        public  List<UiVisualPreset>    Presets { get; }

        public UiPresetCategory( string name, IEnumerable<Preset> presets ) {
            CategoryName = name;
            Presets = new List<UiVisualPreset>( from p in presets orderby p.Name select new UiVisualPreset( p ));
        }
    }

    class UiVisualPreset : PropertyChangedBase {
        private BitmapImage mImage;
        private byte[]      mImageBits;

        public  Preset      Preset { get; }

        public  string      PresetName => Preset.Name;

        public UiVisualPreset( Preset preset ) {
            Preset = preset;
        }

        public BitmapImage PresetImage {
            get {
                if(( mImage == null ) &&
                   ( mImageBits != null )) {
                    using( var stream = new MemoryStream( mImageBits )) {
                        stream.Seek( 0, SeekOrigin.Begin );
                        mImage = new BitmapImage();
                        mImage.BeginInit();
                        mImage.CacheOption = BitmapCacheOption.OnLoad;
                        mImage.StreamSource = stream;
                        mImage.EndInit();
                    }

                    mImageBits = null;
                }

                return mImage;
            }
        }

        public void SetImage( byte[] bits ) {
            mImageBits = bits;

            NotifyOfPropertyChange( () => PresetImage );
        }
    }
}
