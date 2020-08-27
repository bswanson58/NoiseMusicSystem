using System.Collections.Generic;
using System.Linq;
using Q42.HueApi;
using Q42.HueApi.Streaming.Extensions;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Dto {
    public enum GroupLightLocation {
        Front,
        Back,
        Left,
        Right,
        Bottom,
        Center,
        Top
    }

    public class GroupLights {
        public  GroupLightLocation              Location { get; }
        public  IEnumerable<Bulb> Lights { get; }

        public GroupLights( GroupLightLocation location, IEnumerable<Bulb> lights ) {
            Location = location;
            Lights = lights;
        }
    }

    public class EntertainmentGroup {
        public  List<GroupLights>   Lights { get; }

        public EntertainmentGroup( EntertainmentLayer fromLayer, IList<Light> lightList  ) {
            Lights = new List<GroupLights>();

            var lights = new List<EntertainmentLight>( fromLayer.GetFront());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Front, FindBulbs( lights, lightList )));
            }

            lights = new List<EntertainmentLight>( fromLayer.GetBack());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Back, FindBulbs( lights, lightList )));
            }

            lights = new List<EntertainmentLight>( fromLayer.GetLeft());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Left, FindBulbs( lights, lightList )));
            }
            
            lights = new List<EntertainmentLight>( fromLayer.GetRight());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Right, FindBulbs( lights, lightList )));
            }

            lights = new List<EntertainmentLight>( fromLayer.GetBottom());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Bottom, FindBulbs( lights, lightList )));
            }

            lights = new List<EntertainmentLight>( fromLayer.GetCenter());
            if( lights.Any()) {
                Lights.Add( new GroupLights( GroupLightLocation.Center, FindBulbs( lights, lightList )));
            }
        }

        private IEnumerable<Bulb> FindBulbs( IEnumerable<EntertainmentLight> groupLights, IList<Light> lights ) {
            var retValue = new List<Bulb>();

            foreach( var groupLight in groupLights ) {
                var bulb = lights.FirstOrDefault( l => l.Id.Equals( groupLight.Id.ToString()));

                if( bulb != null ) {
                    retValue.Add( new Bulb( bulb.Id, bulb.Name, true ));
                }
            }

            return retValue;
        }

        GroupLights GetLights( GroupLightLocation forLocation ) {
            return Lights.FirstOrDefault( g => g.Location == forLocation );
        }
    }
}
