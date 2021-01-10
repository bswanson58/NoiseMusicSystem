using MagicGradients;
using Noise.RemoteClient.Dto;

namespace Noise.RemoteClient.Interfaces {
    interface ICssStyleProvider {
        void                Initialize( string preferenceName );
        void                SelectNextStyle();
        void                SelectPreviousStyle();

        CssStyle            CurrentStyle { get; }
        GradientCollection  CurrentGradient { get; }
        Dimensions          GradientSize { get; }
    }
}
