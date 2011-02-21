using System.Collections.Generic;
using System.ComponentModel;

namespace Noise.UI.Dto {
	public class ViewSortStrategy {
		public	string		DisplayName { get; private set; }

		public	IEnumerable<SortDescription>	SortDescriptions;

		public ViewSortStrategy( string displayName, IEnumerable<SortDescription> sorting ) {
			DisplayName = displayName;

			SortDescriptions = new List<SortDescription>( sorting );
		}
	}
}
