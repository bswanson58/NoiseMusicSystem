using System;
using System.Collections.Generic;
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
        private string      mImagePath;

        public  Preset      Preset { get; }

        public  string      PresetName => Preset.Name;

        public UiVisualPreset( Preset preset ) {
            Preset = preset;
        }

        public BitmapImage PresetImage {
            get {
                if(( mImage == null ) &&
                   (!String.IsNullOrWhiteSpace( mImagePath ))) {
                    mImage = new BitmapImage( new Uri( mImagePath ));
                }

                return mImage;
            }
        }

        public void SetImage( string path ) {
            mImagePath = path;

            NotifyOfPropertyChange( () => PresetImage );
        }
    }
}
