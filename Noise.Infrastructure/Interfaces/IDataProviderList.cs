using System;
using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProviderList<out T> : IDisposable {
		IEnumerable<T> List { get; }
	}
}
