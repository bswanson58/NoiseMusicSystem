namespace Noise.UI.ViewModels {
	public interface IExplorerViewStrategy {
		bool	Search( string searchText );
		void	ClearCurrentSearch();
	}
}
