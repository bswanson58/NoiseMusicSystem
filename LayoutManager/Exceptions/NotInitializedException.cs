#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class NotInitializedException : Exception
    {
        public NotInitializedException() : base(Resources.NotInitializedErrorMessage)
        {
        }
    }
}