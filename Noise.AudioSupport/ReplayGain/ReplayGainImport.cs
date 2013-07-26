using System;
using System.Runtime.InteropServices;

namespace Noise.AudioSupport.ReplayGain {
	internal class ReplayGain {
		[DllImport( "ReplayGain.dll", CallingConvention = CallingConvention.Cdecl )]
		internal static extern int InitGainAnalysis( long sampleFreq );

		[DllImport( "ReplayGain.dll", CallingConvention = CallingConvention.Cdecl )]
		internal static extern int AnalyzeSamples( double[] leftSamples, double[] rightRamples, UIntPtr numSamples, int numChannels );

		[DllImport( "ReplayGain.dll", CallingConvention = CallingConvention.Cdecl )]
		internal static extern double GetTitleGain();

		[DllImport( "ReplayGain.dll", CallingConvention = CallingConvention.Cdecl )]
		internal static extern double GetAlbumGain();
	}
}
