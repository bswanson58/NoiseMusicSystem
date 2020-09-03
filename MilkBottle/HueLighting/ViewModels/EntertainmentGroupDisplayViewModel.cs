using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using HueLighting.Dto;

namespace HueLighting.ViewModels {
    public class EntertainmentGroupDisplayViewModel {
        public ObservableCollection<UiBulb>   LightList { get; }

        public EntertainmentGroupDisplayViewModel() {
            LightList = new ObservableCollection<UiBulb>();
        }

        public void SetEntertainmentGroup( EntertainmentGroup group ) {
            LightList.Clear();

            if( group != null ) {
                LightList.AddRange( group.AllLights.Select( b => new UiBulb( b, Colors.LemonChiffon )));
            }
        }
    }
}
