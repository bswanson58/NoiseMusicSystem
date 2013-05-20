using System;
using System.Collections.Generic;

namespace Noise.RavenDatabase.Interfaces {
	public interface IQuerySession<out T> : IDisposable where T : class {
		IEnumerable<T>		List { get; }
	}
}
