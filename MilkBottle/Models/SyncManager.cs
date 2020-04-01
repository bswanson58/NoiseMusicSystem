using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MoreLinq;
using ReusableBits.Platform;

namespace MilkBottle.Models {
    class SceneRating {
        public  string[]                    EventValue { get; }
        public  string[]                    SceneValues { get; }
        public  int                         Score { get; }
        public  Func<string,string,int,int> RatingMethod { get; }

        public SceneRating( string[] eventValue, string[] sceneValues, int score, Func<string,string,int,int> ratingMethod ) {
            EventValue = eventValue;
            SceneValues = sceneValues;
            Score = score;
            RatingMethod = ratingMethod;
        }
    }

    class SyncManager : ISyncManager {
        private readonly ISceneProvider     mSceneProvider;
        private readonly IPreferences       mPreferences;

        public SyncManager( ISceneProvider sceneProvider, IPreferences preferences ) {
            mSceneProvider = sceneProvider;
            mPreferences = preferences;
        }

        public PresetScene GetDefaultScene() {
            var retValue = default( PresetScene );
            var sceneList = new List<PresetScene>();
            var preferences = mPreferences.Load<MilkPreferences>();

            mSceneProvider.SelectScenes( list => sceneList.AddRange( list ));

            if(!String.IsNullOrWhiteSpace( preferences.DefaultScene )) {
                retValue = sceneList.FirstOrDefault( s => s.Id.ToString().Equals( preferences.DefaultScene ));
            }

            return retValue ?? sceneList.FirstOrDefault();
        }

        public PresetScene SelectScene( PlaybackEvent forEvent ) {
            if(( forEvent != null ) &&
               ( forEvent.IsValidEvent )) {
                var sceneList = new List<PresetScene>();

                mSceneProvider.SelectScenes( list => sceneList.AddRange( list ));

                var ranking = from scene in sceneList select ( scene, GradeScene( forEvent, scene ));
                var bestRanked = ranking.MaxBy( r => r.Item2 ).Where( r => r.Item2 > 0 ).Select( r => r.scene ).FirstOrDefault();

                return bestRanked ?? GetDefaultScene();
            }
            
            return GetDefaultScene();
        }

        private int GradeScene( PlaybackEvent forEvent, PresetScene scene ) {
            var ratings = from rating in ValuesForScene( scene, forEvent ) select SumSceneRatings( rating );

            return ratings.Sum();
        }

        private int SumSceneRatings( SceneRating rating ) {
            var list = from eventValue in rating.EventValue from sceneValue in rating.SceneValues select rating.RatingMethod( eventValue, sceneValue, rating.Score );

            return list.Sum();
        }

        private int RateValue( string eventValue, string sceneValue, int rating ) {
            return eventValue.Trim().ToLowerInvariant().Contains( sceneValue.Trim().ToLowerInvariant()) ? rating : 0;
        }

        private int RateYearRange( string eventValue, string sceneValue, int rating ) {
            var retValue = 0;

            if((!String.IsNullOrWhiteSpace( eventValue )) &&
               (!String.IsNullOrWhiteSpace( sceneValue )) &&
               (!eventValue.Equals( "0" ))) {
                var yearRange = sceneValue.Split( '-' );
                var startYear = ExtractYear( yearRange[0], true );
                var eventYear = ExtractYear( eventValue );

                if( yearRange.Length == 1 ) {
                    retValue = startYear.Equals( eventYear ) ? rating : 0;
                }
                else {
                    var endYear = ExtractYear( yearRange[1]);

                    if(( String.Compare( startYear, eventYear, StringComparison.Ordinal ) <= 0 ) &&
                       ( String.Compare( endYear, eventYear, StringComparison.Ordinal ) >= 0 )) {
                        retValue = rating;
                    }
                }
            }

            return retValue;
        }

        private string ExtractYear( string input, bool startRange = false ) {
            var retValue = startRange ? "1900" : DateTime.Now.Year.ToString();

            if(!string.IsNullOrWhiteSpace( input )) {
                retValue = input.Trim();
            }

            return retValue;
        }

        private int RateHourRange( string eventValue, string sceneValue, int rating ) {
            var retValue = 0;

            if(!String.IsNullOrWhiteSpace( sceneValue )) {
                var hourRange = sceneValue.Split( '-' );
                var startHour = ExtractHour( hourRange[0], true );
                var eventHour = ExtractHour( eventValue );

                if( hourRange.Length == 1 ) {
                    retValue = ((int)startHour.TotalMinutes).Equals((int)eventHour.TotalMinutes ) ? rating : 0;
                }
                else {
                    var endHour = ExtractHour( hourRange[1]);

                    if(( eventHour >= startHour ) &&
                       ( eventHour <= endHour )) {
                        retValue = rating;
                    }
                }
            }

            return retValue;
        }

        private TimeSpan ExtractHour( string input, bool startRange = false ) {
            var retValue = startRange ? new TimeSpan( 0 ) : new TimeSpan( 1, 0, 0, 0);

            if(!String.IsNullOrWhiteSpace( input )) {
                try {
                    if( input.Contains( ":" )) {
                        retValue = DateTime.ParseExact( input, "H:m", CultureInfo.InvariantCulture ).TimeOfDay;
                    }
                    else {
                        if( Int16.TryParse( input, NumberStyles.Integer, CultureInfo.CurrentCulture.NumberFormat, out var hours )) {
                            retValue = new TimeSpan( 0, hours, 0, 0 );
                        }
                    }
                }
                catch( Exception ) {
                    retValue = new TimeSpan( 0 );
                }
            }

            return retValue;
        }

        private IEnumerable<SceneRating> ValuesForScene( PresetScene scene, PlaybackEvent forEvent ) {
            yield return new SceneRating( SplitString( forEvent.TrackName ), SplitString( scene.TrackNames, PresetScene.cValueSeparator ), 10, RateValue );
            yield return new SceneRating( SplitString( forEvent.AlbumName ), SplitString( scene.AlbumNames, PresetScene.cValueSeparator ), 9, RateValue );
            yield return new SceneRating( SplitString( forEvent.ArtistName ), SplitString( scene.ArtistNames, PresetScene.cValueSeparator ), 8, RateValue );
            yield return new SceneRating( SplitString( forEvent.ArtistGenre ), SplitString( scene.Genres, PresetScene.cValueSeparator ), 7, RateValue );
            yield return new SceneRating( forEvent.TrackTags, SplitString( scene.Tags, PresetScene.cValueSeparator ), 6, RateValue );
            yield return new SceneRating( SplitString( forEvent.PublishedYear.ToString()), SplitString( scene.Years, PresetScene.cValueSeparator ), 6, RateYearRange );
            yield return new SceneRating( SplitString( DateTime.Now.ToString( "HH:mm" )), SplitString( scene.Hours, PresetScene.cValueSeparator ), 5, RateHourRange );
        }

        private string[] SplitString( string input ) {
            return !String.IsNullOrWhiteSpace( input ) ? new []{ input } : new string[]{};
        }

        private string[] SplitString( string input, char separator ) {
            return !String.IsNullOrWhiteSpace( input ) ? input.Split( separator ) : new string[]{};
        }
    }
}
