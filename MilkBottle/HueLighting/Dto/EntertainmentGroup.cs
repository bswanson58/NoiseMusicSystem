﻿using System.Collections.Generic;
using System.Linq;
using Q42.HueApi;
using Q42.HueApi.Streaming.Models;

namespace HueLighting.Dto {
    public enum GroupLightLocation {
        Front = 1,
        Back = 2,
        Left = 3,
        Right = 4,
        Center = 5,
        Ceiling = 6,
        Ground = 7,
        Television = 8
    }

    public class GroupLights {
        public  GroupLightLocation  Location { get; }
        public  List<Bulb>          Lights { get; }

        public GroupLights( GroupLightLocation location ) {
            Location = location;
            Lights = new List<Bulb>();
        }

        public GroupLights( GroupLightLocation location, IEnumerable<Bulb> lights ) {
            Location = location;
            Lights = new List<Bulb>( lights );
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

            foreach( var groupLight in layer ) {
                if( groupLight.LightLocation.X < lowerThird ) {
                    AddLightToGroup( retValue, GroupLightLocation.Right, groupLight, lightList );
                }

                if( groupLight.LightLocation.X > upperThird ) {
                    AddLightToGroup( retValue, GroupLightLocation.Left, groupLight, lightList );
                }

                if( groupLight.LightLocation.Y < lowerThird ) {
                    AddLightToGroup( retValue, GroupLightLocation.Front, groupLight, lightList );
                }

                if( groupLight.LightLocation.Y > upperThird ) {
                    AddLightToGroup( retValue, GroupLightLocation.Back, groupLight, lightList );
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

                if( groupLight.LightLocation.X > lowerThird && groupLight.LightLocation.X < upperThird &&
                    groupLight.LightLocation.Y > lowerThird && groupLight.LightLocation.Y < upperThird ) {
                    AddLightToGroup( retValue, GroupLightLocation.Center, groupLight, lightList );
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
