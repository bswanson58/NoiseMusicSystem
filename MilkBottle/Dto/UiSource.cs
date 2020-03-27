using MilkBottle.Entities;

namespace MilkBottle.Dto {
    class UiSource {
        public  SceneSource Source { get; }
        public  string      Title { get; }

        public UiSource( string title, SceneSource source ) {
            Source = source;
            Title = title;
        }
    }
}
