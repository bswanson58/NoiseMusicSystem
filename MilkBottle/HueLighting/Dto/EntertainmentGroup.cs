using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MilkBottle.Infrastructure.Dto;
using Q42.HueApi;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Dto {
    [DebuggerDisplay("GroupLights: {" + nameof( Location ) + "}")]
    public class GroupLights {
        public  GroupLightLocation  Location { get; }
        public  List<Bulb>          Lights { get; }

        public GroupLights( GroupLightLocation location ) {
            Location = location;
            Lights = new List<Bulb>();
        }
    }

    public class EntertainmentGroup {
        public  List<GroupLights>   Lights { get; }

        public EntertainmentGroup( EntertainmentLayer fromLayer, IList<Light> lightList  ) {
            Lights = new List<GroupLights>( from g in BuildLightGroups( fromLayer, lightList ) orderby g.Location select g );
        }

        private IEnumerable<GroupLights> BuildLightGroups( EntertainmentLayer layer, IList<Light> lightList ) {
            var retValue = new List<GroupLights>();
            var lowerThird = -0.5;
            var upperThird = 0.5;

            if( layer != null ) {
                foreach( var groupLight in layer ) {
                    if( groupLight.LightLocation.X < lowerThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Left, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.X > upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Right, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Y < lowerThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Back, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Y > upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Front, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Z < lowerThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Ground, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Z > upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Ceiling, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Z > lowerThird && groupLight.LightLocation.Z < upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Television, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.Y > lowerThird && groupLight.LightLocation.Y < upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.CenterLeftRight, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.X > lowerThird && groupLight.LightLocation.X < upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.CenterFrontBack, groupLight, lightList );
                    }

                    if( groupLight.LightLocation.X > lowerThird && groupLight.LightLocation.X < upperThird &&
                        groupLight.LightLocation.Y > lowerThird && groupLight.LightLocation.Y < upperThird ) {
                        AddLightToGroup( retValue, GroupLightLocation.Center, groupLight, lightList );
                    }
                }
            }

            return retValue;
        }

        private void AddLightToGroup( IList<GroupLights> groupLights, GroupLightLocation location, EntertainmentLight light, IList<Light> lightList ) {
            var group = groupLights.FirstOrDefault( g => g.Location.Equals( location ));

            if( group == null ) {
                group = new GroupLights( location );

                groupLights.Add( group );
            }

            group.Lights.Add( MakeBulb( light, lightList ));

            var sortedList = new List<Bulb>( group.Lights );
            group.Lights.Clear();
            group.Lights.AddRange( from bulb in sortedList orderby bulb.Name select bulb );
        }

        private Bulb MakeBulb( EntertainmentLight light, IList<Light> lightList ) {
            var retValue = default( Bulb );
            var bulb = lightList.FirstOrDefault( l => l.Id.Equals( light.Id.ToString()));

            if( bulb != null ) {
                retValue = new Bulb( bulb.Id, bulb.Name, true );
            }

            return retValue;
        }

        public GroupLights GetLights( GroupLightLocation forLocation ) {
            return Lights.FirstOrDefault( g => g.Location.Equals( forLocation ));
        }
    }
}
