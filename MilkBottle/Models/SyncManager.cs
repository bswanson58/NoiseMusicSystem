using System;
using System.Collections.Generic;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MoreLinq;
using ReusableBits.Platform;

namespace MilkBottle.Models {
    class SceneRating {
        public  string[]    EventValue { get; }
        public  string[]    SceneValues { get; }
        public  int         Score { get; }

        public SceneRating( string[] eventValue, string[] sceneValues, int score ) {
            EventValue = eventValue;
            SceneValues = sceneValues;
            Score = score;
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
            var sceneList = new List<PresetScene>();

            mSceneProvider.SelectScenes( list => sceneList.AddRange( list ));

            var ranking = from scene in sceneList select ( scene, GradeScene( forEvent, scene ));
            var bestRanked = ranking.MaxBy( r => r.Item2 ).Where( r => r.Item2 > 0 ).Select( r => r.scene ).FirstOrDefault();

            return bestRanked ?? GetDefaultScene();
        }

        private int GradeScene( PlaybackEvent forEvent, PresetScene scene ) {
            var ratings = from rating in ValuesForScene( scene, forEvent ) select SumSceneRatings( rating );

            return ratings.Sum();
        }

        private int SumSceneRatings( SceneRating rating ) {
            var list = from eventValue in rating.EventValue from sceneValue in rating.SceneValues select RateValue( eventValue, sceneValue, rating.Score );

            return list.Sum();
        }

        private int RateValue( string eventValue, string sceneValue, int rating ) {
            return eventValue.Trim().ToLowerInvariant().Contains( sceneValue.Trim().ToLowerInvariant()) ? rating : 0;
        }

        private IEnumerable<SceneRating> ValuesForScene( PresetScene scene, PlaybackEvent forEvent ) {
            yield return new SceneRating( SplitString( forEvent.TrackName ), SplitString( scene.TrackNames, PresetScene.cValueSeparator ), 10 );
            yield return new SceneRating( SplitString( forEvent.AlbumName ), SplitString( scene.AlbumNames, PresetScene.cValueSeparator ), 9 );
            yield return new SceneRating( SplitString( forEvent.ArtistName ), SplitString( scene.ArtistNames, PresetScene.cValueSeparator ), 8 );
            yield return new SceneRating( SplitString( forEvent.ArtistGenre ), SplitString( scene.Genres, PresetScene.cValueSeparator ), 7 );
            yield return new SceneRating( forEvent.TrackTags, SplitString( scene.Tags, PresetScene.cValueSeparator ), 6 );
        }

        private string[] SplitString( string input ) {
            return !String.IsNullOrWhiteSpace( input ) ? new []{ input } : new string[]{};
        }

        private string[] SplitString( string input, char separator ) {
            return !String.IsNullOrWhiteSpace( input ) ? input.Split( separator ) : new string[]{};
        }
    }
}
