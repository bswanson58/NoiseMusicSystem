using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// from: http://learnwpf.com/post/2011/03/02/Come-on-Feel-the-Noizee28093adding-noise-to-your-WPF-applications.aspx

namespace Noise.UI.Behaviours {
	public class NoiseGenerator : DependencyObject {
		private static readonly DependencyPropertyKey NoiseImageKey = 
			DependencyProperty.RegisterReadOnly( "NoiseImage", typeof( ImageSource ), typeof( NoiseGenerator ), new PropertyMetadata( null ));

		public NoiseGenerator() {
			Colors = new ObservableCollection<ColorFrequency>();
			Size = 100;
			GenerateNoiseBitmap();
		}

		private void OnColorsCollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e ) {
			GenerateNoiseBitmap();
		}

		public ImageSource NoiseImage {
			get {
				return (ImageSource)GetValue( NoiseImageKey.DependencyProperty );
			}
		}

		public int Size { get; set; }

		private ObservableCollection<ColorFrequency> mColorFrequency;
		public ObservableCollection<ColorFrequency> Colors {
			get { return mColorFrequency; }
			set {
				if( mColorFrequency == value ) {
					return;
				}
				if( mColorFrequency != null ) {
					Colors.CollectionChanged -= OnColorsCollectionChanged;
				}
				mColorFrequency = value;
				if( mColorFrequency != null ) {
					Colors.CollectionChanged += OnColorsCollectionChanged;
				}
			}
		}


		// BitmapSource generation code coutesy of http://social.msdn.microsoft.com/forums/en-US/wpf/thread/56364b28-1277-4be8-8d45-78788cc4f2d7/
		private void GenerateNoiseBitmap() {
			if( Colors == null || Colors.Count == 0 ) {
				SetValue( NoiseImageKey, null );
				return;
			}

			try {
				var rnd = new Random();
				var colors = Colors.Select( value => value.Color ).ToList();
				var totalFrequency = Colors.Sum( a => a.Frequency );
				var frequencyMap = GetFrequencyMap();


				var palette = new BitmapPalette( colors );

				PixelFormat pf = PixelFormats.Bgra32;
				int width = Size;
				int height = width;

				int stride = ( width * pf.BitsPerPixel ) / 8;

				var pixels = new byte[height * stride];


				for( int i = 0; i < height * stride; i += ( pf.BitsPerPixel / 8 ) ) {
					var color = GetWeightedRandomColor( totalFrequency, frequencyMap, rnd );

					pixels[i] = color.B;
					pixels[i + 1] = color.G;
					pixels[i + 2] = color.R;
					pixels[i + 3] = color.A;
				}


				var image = BitmapSource.Create( width, height, 96, 96, pf, palette, pixels, stride );
				SetValue( NoiseImageKey, image );
			}
			catch( ArgumentException ) {

			}
		}

		private Color GetWeightedRandomColor( int totalFrequency, List<Tuple<int, Color>> frequencyMap, Random rnd ) {
			int value = rnd.Next( 0, totalFrequency );
			for( int i = 0; i < frequencyMap.Count - 2; i++ ) {
				if( frequencyMap[i].Item1 < value && frequencyMap[i + 1].Item1 >= value ) {
					return frequencyMap[i].Item2;
				}
			}
			return frequencyMap.Last().Item2;
		}

		private List<Tuple<int, Color>> GetFrequencyMap() {
			var frequencyMap = new List<Tuple<int, Color>>();
			int counter = 0;
			foreach( var item in Colors ) {
				frequencyMap.Add( new Tuple<int, Color>( counter, item.Color ) );
				counter += item.Frequency;
			}
			return frequencyMap;
		}
	}

	public class ColorFrequency {
		public Color Color { get; set; }
		public int Frequency { get; set; }
	}
}
