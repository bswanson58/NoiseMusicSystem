#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class LayoutControlNotUserControlException : Exception
    {
        public LayoutControlNotUserControlException() : base(Resources.LayoutControlNotUserControlErrorMessage)
        {
        }
    }
}