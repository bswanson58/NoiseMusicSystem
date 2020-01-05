using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ReusableBits.Ui.Utility {
	// from: https://code.google.com/p/gishu-util/source/browse/WPF/Utilities/DepObjExtn.cs
	public static class DependencyObjectExtensions {
		/// <summary>
		/// Get a list of descendant dependencyobjects of the specified type and which meet the criteria
		/// as specified by the predicate
		/// </summary>
		/// <typeparam name="T">The type of child you want to find</typeparam>
		/// <param name="parent">The dependency object whose children you wish to scan</param>
		/// <param name="predicate">The child object is selected if the predicate evaluates to true</param>
		/// <returns>The first matching descendant of the specified type</returns>
		/// <remarks> usage: myWindow.FindChildren<StackPanel>( child => child.Name == "myPanel" ) </StackPanel></remarks>
		public static IEnumerable<T> FindChildren<T>( this DependencyObject parent, Func<T, bool> predicate ) where T : DependencyObject {
			var children = new List<DependencyObject>();

			if(( parent is Visual ) ||
			   ( parent is Visual3D )) {
				var visualChildrenCount = VisualTreeHelper.GetChildrenCount( parent );

				for( int childIndex = 0; childIndex < visualChildrenCount; childIndex++ ) {
					children.Add( VisualTreeHelper.GetChild( parent, childIndex ) );
				}
			}

			foreach( var logicalChild in LogicalTreeHelper.GetChildren( parent ).OfType<DependencyObject>()) {
				if(!children.Contains( logicalChild )) {
					children.Add( logicalChild );
				}
			}

			foreach( var child in children ) {
				var typedChild = child as T;

				if(( typedChild != null ) &&
				   ( predicate.Invoke( typedChild ))) {
					yield return typedChild;
				}

				foreach( var foundDescendant in FindChildren( child, predicate )) {
					yield return foundDescendant;
				}
			}
		}

        public static IEnumerable<T> FindChildren<T>( this DependencyObject depObj ) where T : DependencyObject {
            if( depObj != null ) {
                for( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ ) {
                    var child = VisualTreeHelper.GetChild( depObj, i );

                    if( child is T dependencyObject ) {
                        yield return dependencyObject;
                    }

                    foreach( T childOfChild in FindChildren<T>( child )) {
                        yield return childOfChild;
                    }
                }
            }
        }
	}
}
