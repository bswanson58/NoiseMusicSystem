#region Using Directives

using System;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Exceptions
{
    public class EmptyLayoutsCollectionException : Exception
    {
        public EmptyLayoutsCollectionException() : base(Resources.EmptyLayoutsCollectionErrorMessage)
        {
        }
    }
}