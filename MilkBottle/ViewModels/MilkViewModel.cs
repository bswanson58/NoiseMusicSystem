using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.ViewModels {
    class MilkViewModel {
        private readonly IMilkController    mMilkController;

        public MilkViewModel( IMilkController milkController ) {
            mMilkController = milkController;
        }

        public void Initialize( GLControl glControl ) {
            mMilkController.Initialize( glControl );
        }

        public void StartVisualization() {
            mMilkController.StartVisualization();
        }

        public void OnSizeChanged( int width, int height ) {
            mMilkController.OnSizeChanged( width, height );
        }
    }
}
