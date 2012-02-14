using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure.Configuration;
using Noise.UI.Adapters;
using Noise.UI.Behaviours;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Controls;
using Noise.UI.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ExplorerFilterInfo : InteractionRequestData<LibraryExplorerFilter> {
		public ExplorerFilterInfo( LibraryExplorerFilter viewModel ) : base( viewModel ) { }
	}

	internal class ExplorerSortInfo : InteractionRequestData<ArtistAlbumConfigViewModel> {
		public ExplorerSortInfo( ArtistAlbumConfigViewModel viewModel ) : base( viewModel ) { }
	}

	public class LibraryExplorerViewModel : AutomaticCommandBase {
		private const string	cVisualStateNormal		= "Normal";
		private const string	cVisualStateIndex		= "DisplayIndex";
		private const string	cVisualStateStrategy	= "DisplayStrategy";

		private IExplorerViewStrategy					mViewStrategy;
		private readonly PlaybackFocusTracker			mFocusTracker;
		private readonly List<string>					mSearchOptions;
		private readonly LibraryExplorerFilter			mExplorerFilter;
		private readonly bool							mEnableSortPrefixes;
		private readonly List<string>					mSortPrefixes;
		private readonly BindableCollection<UiTreeNode>	mTreeItems;
		private readonly BindableCollection<IndexNode>	mIndexItems;
		private readonly BindableCollection<SortDescription>	mTreeSortDescriptions; 
		private	readonly InteractionRequest<ExplorerFilterInfo>	mExplorerFiltersEdit;
		private	readonly InteractionRequest<ExplorerSortInfo>	mExplorerSortRequest;

		public	IEnumerable<IExplorerViewStrategy>		ViewStrategies { get; private set; }

		public LibraryExplorerViewModel( IEnumerable<IExplorerViewStrategy> viewStrategies, PlaybackFocusTracker focusTracker ) {
			ViewStrategies = viewStrategies;
			mFocusTracker = focusTracker;

			mExplorerFilter = new LibraryExplorerFilter { IsEnabled = false };
			mTreeItems = new BindableCollection<UiTreeNode>();
			mIndexItems = new BindableCollection<IndexNode>();
			mTreeSortDescriptions = new BindableCollection<SortDescription>();
			mExplorerFiltersEdit = new InteractionRequest<ExplorerFilterInfo>();
			mExplorerSortRequest = new InteractionRequest<ExplorerSortInfo>();

			mSortPrefixes = new List<string>();
			mSearchOptions = new List<string>();

			VisualStateName = cVisualStateNormal;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mEnableSortPrefixes = configuration.EnableSortPrefixes;

				if( mEnableSortPrefixes ) {
					mSortPrefixes.AddRange( configuration.SortPrefixes.Split( '|' ));
				}
			}

			var strategyList = ViewStrategies.ToList();
			foreach( var strategy in strategyList ) {
				strategy.Initialize( this );
				strategy.UseSortPrefixes( mEnableSortPrefixes, mSortPrefixes );
			}

			SelectedStrategy = ( from strategy in strategyList where strategy.IsDefaultStrategy select strategy ).FirstOrDefault();
		}

		public IExplorerViewStrategy SelectedStrategy {
			get{ return( mViewStrategy ); }
			set{ 
				VisualStateName = cVisualStateNormal;

				ActivateStrategy( value );
			}
		}

		private void ActivateStrategy( IExplorerViewStrategy strategy ) {
			mTreeItems.Clear();
			mIndexItems.Clear();
			mSearchOptions.Clear();

			if( mViewStrategy != null ) {
				mViewStrategy.Deactivate();
			}

			mViewStrategy = strategy;
			if( mViewStrategy != null ) {
				mViewStrategy.Activate();

				UpdateTree();
			}
		}

		private void UpdateTree() {
			PopulateTree( BuildTree());
			UpdateIndex();
		}

		private IEnumerable<UiTreeNode> BuildTree() {
			return( mViewStrategy.BuildTree( mExplorerFilter ));
		}

		private void PopulateTree( IEnumerable<UiTreeNode> newNodes ) {
			mTreeItems.Clear();
	 		mTreeItems.AddRange( newNodes );
		}

		private void UpdateIndex() {
			if(( mViewStrategy != null ) &&
			   ( mTreeItems != null )) {
				mIndexItems.Clear();
				mIndexItems.AddRange( mViewStrategy.BuildIndex( mTreeItems ));
			}
		}

		public BindableCollection<UiTreeNode> TreeData {
			get { return( mTreeItems ); }
		}

		internal void SetTreeSortDescription( IEnumerable<SortDescription> descriptions ) {
			mTreeSortDescriptions.Clear();
			mTreeSortDescriptions.AddRange( descriptions );
		} 
	
		public BindableCollection<SortDescription> SortDescriptions {
			get{ return( mTreeSortDescriptions ); }
		}
 
		public DataTemplate TreeViewItemTemplate {
			get{ return( Get( () => TreeViewItemTemplate )); }
			set{ Set( () => TreeViewItemTemplate, value ); }
		}

		public IInteractionRequest ExplorerSortRequest {
			get{ return( mExplorerSortRequest ); }
		}

		public void Execute_ConfigureView() {
			if( mViewStrategy != null ) {
				var dialogModel = mViewStrategy.ConfigureSortRequest();

				mExplorerSortRequest.Raise( new ExplorerSortInfo( dialogModel ), OnSortRequest );
			}
		}

		private void OnSortRequest( ExplorerSortInfo confirmation ) {
			if(( confirmation.Confirmed ) &&
			   ( mViewStrategy != null )) {
				mViewStrategy.SortRequest( confirmation.ViewModel );
			}
		}

		public bool CanExecute_ConfigureView() {
			return(( mViewStrategy != null ) &&
				   ( mViewStrategy.CanConfigureViewSort()));
		}

		public string SearchText {
			get{ return( Get( () => SearchText )); }
			set {
				Set( () => SearchText, value );

				mViewStrategy.ClearCurrentSearch();
			}
		}

		public IInteractionRequest ExplorerFilterRequest {
			get{ return( mExplorerFiltersEdit ); }
		}

		public void Execute_Filter() {
			mExplorerFiltersEdit.Raise( new ExplorerFilterInfo( mExplorerFilter ), OnFiltersEdited );
		}

		private void OnFiltersEdited( ExplorerFilterInfo confirmation ) {
			if( confirmation.Confirmed ) {
				UpdateTree();
			}
		}

		public void Execute_Search( EventCommandParameter<object, RoutedEventArgs> args ) {
			var searchArgs = args.EventArgs as SearchEventArgs;

			if( searchArgs != null ) {
				mViewStrategy.Search( searchArgs.Keyword, searchArgs.Sections );
			}
		}

		[DependsUpon( "SearchText" )]
		public bool CanExecute_Search() {
			return(!string.IsNullOrWhiteSpace( SearchText ));
		}

		public List<string> SearchOptions {
			get{ return( mSearchOptions ); }
		}

		public bool HaveSearchOptions {
			get{ return(( mSearchOptions != null ) && ( mSearchOptions.Count > 0 )); }
		}

		public string VisualStateName {
			get{ return( Get( () => VisualStateName )); }
			set{ Set( () => VisualStateName, value ); }
		}

		public void Execute_ToggleStrategyDisplay() {
			if(( VisualStateName != cVisualStateNormal ) &&
			   ( VisualStateName != cVisualStateStrategy )) {
				VisualStateName = cVisualStateNormal;
			}

			VisualStateName = VisualStateName == cVisualStateNormal ? cVisualStateStrategy : cVisualStateNormal;
		}

		public void Execute_ToggleIndexDisplay() {
			if(( VisualStateName != cVisualStateNormal ) &&
			   ( VisualStateName != cVisualStateIndex )) {
				VisualStateName = cVisualStateNormal;
			}

			VisualStateName = VisualStateName == cVisualStateNormal ? cVisualStateIndex : cVisualStateNormal;
		}

		public IEnumerable<IndexNode> IndexData {
			get { return( mIndexItems ); }
		}

		public IndexNode SelectedIndex {
			get{ return( null ); }
			set {
				VisualStateName = cVisualStateNormal;

				if( value != null ) {
					value.DisplayNode();
				}
			}
		}
	}
}
