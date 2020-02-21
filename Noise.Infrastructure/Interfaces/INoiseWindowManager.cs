using System.Collections.ObjectModel;
using System.Windows;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface INoiseWindowManager {
        void	Initialize( Window shell );
        void	ActivateShell();
        void	Shutdown();

        void    ChangeWindowLayout( string toLayout );

        ObservableCollection<UiCompanionApp>    CompanionApplications { get; }
    }
}
