using HueLighting.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    internal class HubViewModel : PropertyChangeBase {
        public  HubInformation  Hub { get; }

        public  string          IpAddress => Hub.IpAddress;
        public  string          HubName => Hub.BridgeName;
        public  bool            IsRegistered => Hub.IsAppRegistered;

        public HubViewModel( HubInformation hub ) {
            Hub = hub;
        }
    }
}
