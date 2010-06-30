#region Using Directives

using System;
using System.Windows;

#endregion

namespace Composite.Layout
{
    public interface IView
    {
        string TypeName { get; set; }
        Type Type { get; set; }
        string RegionName { get; set; }
        Visibility Visibility { get; set; }
    }
}