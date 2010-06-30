#region Using Directives

using System;
using System.Windows;

#endregion

namespace Composite.Layout
{
    public class View : DependencyObject, IView
    {
        public static readonly DependencyProperty RegionNameProperty = DependencyProperty.Register(
            "RegionName", typeof (string), typeof (View), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register(
            "TypeName", typeof (string), typeof (View), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof (Type), typeof (View), null);

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(
            "Visibility", typeof (Visibility), typeof (View), new PropertyMetadata(Visibility.Visible));

        #region IView Members

        public Visibility Visibility
        {
            get { return (Visibility) GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public string TypeName
        {
            get { return (string) GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public Type Type
        {
            get { return (Type) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string RegionName
        {
            get { return (string) GetValue(RegionNameProperty); }
            set { SetValue(RegionNameProperty, value); }
        }

        #endregion
    }
}