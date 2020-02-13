﻿using System;
using Caliburn.Micro;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.ViewModels {
    class MilkViewModel : IDisposable {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IMilkController    mMilkController;


        public MilkViewModel( IMilkController milkController, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
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

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
