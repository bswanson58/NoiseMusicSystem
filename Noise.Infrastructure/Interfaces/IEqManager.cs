using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IEqManager {
		bool	Initialize( string presetFile );
		bool	SaveEq( ParametricEqualizer eq, bool eqEnabled );

		IEnumerable<ParametricEqualizer>	EqPresets { get; }
		ParametricEqualizer					CurrentEq { get; set; }
	}
}
