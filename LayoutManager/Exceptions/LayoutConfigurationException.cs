#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class LayoutConfigurationException : Exception
    {
        public LayoutConfigurationException() : base(Resources.LayoutConfigurationErrorMessage)
        {
        }
    }
}