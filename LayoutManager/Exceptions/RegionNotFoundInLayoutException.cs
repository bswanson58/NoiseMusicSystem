#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class RegionNotFoundInLayoutException : Exception
    {
        public RegionNotFoundInLayoutException(string regionName, string layoutName)
            : base(string.Format(Resources.RegionNotFoundInLayoutErrorMessage, regionName, layoutName))
        {
        }
    }
}