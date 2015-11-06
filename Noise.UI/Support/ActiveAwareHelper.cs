using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism;
using ReusableBits.Ui.Utility;

namespace Noise.UI.Support {
	public class ActiveAwareHelper {
		private readonly List<IActiveAware>	mChildModels; 
		private readonly Control			mParent;
		private readonly EventHandler		mActiveChanged;
		private bool						mIsActive;

		public ActiveAwareHelper( Control parent, EventHandler activeChanged ) {
			mParent = parent;
			mActiveChanged = activeChanged;
			mChildModels = new List<IActiveAware>();

			mParent.Initialized += OnInitialized;
		}

		private void OnInitialized( object sender, EventArgs args ) {
			var activatableViewList = ( sender as DependencyObject ).FindChildren<Control>( item => ( item.DataContext is IActiveAware ));

			mChildModels.AddRange(( from view in activatableViewList select view.DataContext as IActiveAware ).Distinct());

			IsActive = mIsActive;
			mParent.Loaded -= OnInitialized;
		}

		public bool IsActive {
			get{ return( mIsActive ); }
			set {
				mIsActive = value;
				mActiveChanged( mParent, new EventArgs());

				mChildModels.ForEach( vm => vm.IsActive = value );
			}
		}
	}
}
