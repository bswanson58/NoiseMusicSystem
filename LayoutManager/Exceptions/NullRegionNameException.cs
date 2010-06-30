#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class NullRegionNameException : Exception
    {
        public NullRegionNameException()
            : base(Resources.NullRegionNameErrorMessage)
        {
        }
    }
}