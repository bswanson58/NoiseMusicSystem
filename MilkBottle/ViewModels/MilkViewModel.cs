using System.Windows;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.ViewModels {
    class MilkViewModel {
        private readonly IInitializationController  mInitializationController;
        private readonly IMilkController            mMilkController;


        public MilkViewModel( IInitializationController initializationController, IMilkController milkController ) {
            mInitializationController = initializationController;
            mMilkController = milkController;
        }

        public void Initialize( GLControl glControl ) {
            mInitializationController.ContextReady( glControl );
        }

        public void OnSizeChanged( Size size ) {
            mMilkController.OnSizeChanged( size );
        }
    }
}
