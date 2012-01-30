using System;

namespace Noise.Infrastructure.Interfaces {
	public interface IBlendableViewModelFactory {
		Type	ViewModelType { get; }

		object	CreateViewModel();
	}
}
