#region Using Directives

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace Composite.Layout
{
    public interface ILayout
    {
        string Name { get; set; }
        string Fullname { get; set; }
        string Filename { get; set; }
        bool IsDefault { get; set; }
        ContentControl ContentControl { get; set; }
        string TypeName { get; set; }
        Type Type { get; set; }
        List<IView> Views { get; }
        string Description { get; set; }
        ImageSource ThumbnailSource { get; set; }
    }
}