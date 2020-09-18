using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using MilkBottle.Entities;
using Prism.Commands;

namespace MilkBottle.Dto {
    class UiPresetCategory {
        public  string                  CategoryName { get; }
        public  List<UiVisualPreset>    Presets { get; }

        public UiPresetCategory( string name, IEnumerable<Preset> presets, Action<UiVisualPreset> onDisplayPreset ) {
            CategoryName = name;
            Presets = new List<UiVisualPreset>( from p in presets orderby p.Name select new UiVisualPreset( p, onDisplayPreset ));
        }
    }

    class UiVisualPreset : PropertyChangedBase {
        private readonly Action<UiVisualPreset> mOnDisplayPreset;
        private BitmapImage                     mImage;
        private byte[]                          mImageBits;

        public  Preset                          Preset { get; }
        public  string                          PresetName => Preset.Name;
        public  DelegateCommand                 DisplayActivePreset { get; }

        public  Point                           Location { get; set; }

        public UiVisualPreset( Preset preset, Action<UiVisualPreset> onDisplayPreset ) {
            Preset = preset;
            mOnDisplayPreset = onDisplayPreset;

            DisplayActivePreset = new DelegateCommand( OnDisplayActivePreset );
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

        private void OnDisplayActivePreset() {
            mOnDisplayPreset?.Invoke( this );
        }
    }
}
