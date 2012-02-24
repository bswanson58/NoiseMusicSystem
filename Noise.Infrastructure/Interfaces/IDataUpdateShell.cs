using System;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataUpdateShell<out T> : IDisposable {
		T		Item { get; }
		void	Update();
	}
}
