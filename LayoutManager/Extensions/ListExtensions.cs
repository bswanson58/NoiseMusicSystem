#region Using Directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Composite.Layout.Extensions
{
    public static class ListExtensions
    {
        public static bool ContainsName(this List<ILayout> layouts, string name)
        {
            var count = layouts.Count(c => c.Name.Equals(name));
            return count != 0;
        }
    }
}