using MilkBottle.Entities;

namespace MilkBottle.Dto {
    class UiCycling {
        public  PresetCycling   Cycling { get; }
        public  string          Title { get; }

        public UiCycling( string title, PresetCycling cycling ) {
            Cycling = cycling;
            Title = title;
        }
    }
}
