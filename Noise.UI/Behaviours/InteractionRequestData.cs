using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Noise.UI.Behaviours {
	public class InteractionRequestData<T> : Confirmation where T : class {
		public	T	ViewModel { get; private set; }

		public InteractionRequestData( T viewModel ) {
			ViewModel = viewModel;
			Content = viewModel;
		}  
	}
}
