using Xamarin.Forms;

namespace Noise.RemoteClient.Resources {
    public class ColorReference {
        public Color Color { get; set; } = Color.Black;

        public static implicit operator Color( ColorReference colorReference ) => colorReference.Color;
    }
}
