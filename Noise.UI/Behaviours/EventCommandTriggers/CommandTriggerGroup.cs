using System.Collections.Generic;
using System.Windows;

namespace Noise.UI.Behaviours.EventCommandTriggers {
	public sealed class CommandTriggerGroup : FreezableCollection<CommandTrigger>, ICommandTrigger {
		private readonly HashSet<ICommandTrigger> mInitList = new HashSet<ICommandTrigger>();

		void ICommandTrigger.Initialize( FrameworkElement source ) {
			foreach( var child in this ) {
				if( !mInitList.Contains( child ) ) {
					InitializeCommandSource( source, child );
				}
			}
		}

		private void InitializeCommandSource( FrameworkElement source, ICommandTrigger child ) {
			child.Initialize( source );
			mInitList.Add( child );
		}
	}
}
