using ReusableBits.Interfaces;
using ReusableBits.Patterns;
using ReusableBits.Support;

namespace Noise.Infrastructure.Support {
	public class DatabaseIdentityProvider : SingletonBase<IIdentityProvider, IdentityProvider> {
	}
}
