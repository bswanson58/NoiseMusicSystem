using MagicGradients;

namespace Noise.RemoteClient.Interfaces {
    interface ICssStyleProvider {
        void                Initialize( string preferenceName );
        void                SelectNextStyle();

        GradientCollection  CurrentGradient { get; }
        Dimensions          GradientSize { get; }
    }
}
