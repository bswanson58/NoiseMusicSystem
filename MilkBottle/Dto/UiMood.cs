using MilkBottle.Entities;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiMood : PropertyChangeBase {
        private readonly Mood   mMood;

        public  string          Name => mMood.Name;

        public UiMood( Mood mood ) {
            mMood = mood;
        }
    }
}
