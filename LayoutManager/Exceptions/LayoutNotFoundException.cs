#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class LayoutNotFoundException : Exception
    {
        public LayoutNotFoundException(string layoutName)
            : base(string.Format(Resources.LayoutNotFoundErrorMessage, layoutName))
        {
        }
    }
}