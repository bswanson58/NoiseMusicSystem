using System;
using System.Linq;

namespace Noise.RavenDatabase.Interfaces {
	public interface IQuerySession<out T> : IDisposable where T : class {
		IQueryable<T>		Query(); 
	}
}
