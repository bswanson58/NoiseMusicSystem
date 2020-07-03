using System.Collections.ObjectModel;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IIpcManager {
        ObservableCollection<UiCompanionApp>    CompanionApplications { get; }
    }
}
