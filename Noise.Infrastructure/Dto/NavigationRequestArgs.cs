namespace Noise.Infrastructure.Dto {
	public class NavigationRequestArgs {
		public	string		RequestingViewName { get; private set; }

		public NavigationRequestArgs( string requestingView ) {
			RequestingViewName = requestingView;
		}
	}
}
