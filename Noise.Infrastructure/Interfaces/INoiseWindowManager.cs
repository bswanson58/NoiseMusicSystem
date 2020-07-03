using System.Windows;

namespace Noise.Infrastructure.Interfaces {
    public interface INoiseWindowManager {
        void	Initialize( Window shell );
        void	ActivateShell();
        void    DeactivateShell();
        void	Shutdown();

        void    ChangeWindowLayout( string toLayout );
    }
}
