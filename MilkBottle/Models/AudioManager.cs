using System;
using MilkBottle.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MilkBottle.Models {
    public class AudioManager : IAudioManager {
        private WaveFormat                  mAudioFormat;
        private WasapiLoopbackCapture       mCapture;
        private Action<byte[], int, int>    mOnAudioData;

        public bool InitializeAudioCapture() {
            var enumerator = new MMDeviceEnumerator();
            var endPoint = enumerator.GetDefaultAudioEndpoint( DataFlow.Render, Role.Console );
            var audioDevice = endPoint.AudioClient;

            mAudioFormat = audioDevice.MixFormat;
            mCapture = new WasapiLoopbackCapture( endPoint );

            return mAudioFormat.BitsPerSample == 32;
        }

        public void StartCapture( Action<byte[], int, int> onAudioData ) {
            mOnAudioData = onAudioData;

            if( mCapture.CaptureState != CaptureState.Capturing ) {
                mCapture.DataAvailable += OnAudioCapture;
                mCapture.StartRecording();
            }
        }

        public void StopCapture() {
            if( mCapture.CaptureState == CaptureState.Capturing ) {
                mCapture.StopRecording();
                mCapture.DataAvailable -= OnAudioCapture;
            }
        }

        private void OnAudioCapture( object sender, WaveInEventArgs args ) {
            if( args.BytesRecorded > 0 ) {
                var samples = args.BytesRecorded / ( mAudioFormat.BitsPerSample / 8 );

                mOnAudioData?.Invoke( args.Buffer, samples, mAudioFormat.Channels );
            }
        }

        public void Dispose() {
            StopCapture();

            mCapture?.Dispose();
        }
    }
}
