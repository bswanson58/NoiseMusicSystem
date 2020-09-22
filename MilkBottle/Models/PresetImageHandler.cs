using System;
using System.IO;
using MilkBottle.Entities;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetImageHandler : IPresetImageHandler {
        private byte[]          mDefaultImage;

        public byte[] GetPresetImage( Preset preset ) {
            var retValue = default( byte[]);
            var imagePath = Path.ChangeExtension( preset.Location, ".jpg" );

            if(!String.IsNullOrWhiteSpace( imagePath )) {
                if( File.Exists( imagePath )) {
                    using ( var stream = File.OpenRead( imagePath )) {
                        retValue = new byte[stream.Length];

                        stream.Read( retValue, 0, retValue.Length );
                        stream.Close();
                    }
                }
                else {
                    retValue = GetDefaultImage();
                }
            }

            return retValue;
        }

        private byte[] GetDefaultImage() {
            if( mDefaultImage == null ) {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                using( var stream = assembly.GetManifestResourceStream( assembly.GetName().Name + ".Resources.Default Preset Image.png" )) {
                    if( stream != null ) {
                        mDefaultImage = new byte[stream.Length];

                        stream.Read( mDefaultImage, 0, mDefaultImage.Length);
                    }
                }
            }

            return mDefaultImage;
        }
    }
}
