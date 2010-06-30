#region Using Directives

using System.Windows;

#endregion

namespace Composite.Layout
{
    public class ViewModel : View, IViewModel
    {
        public static readonly DependencyProperty ViewPropertyProperty = DependencyProperty.Register(
            "ViewProperty", typeof (string), typeof (ViewModel), new PropertyMetadata(string.Empty));

        #region IViewModel Members

        public string ViewProperty
        {
            get { return (string) GetValue(ViewPropertyProperty); }
            set { SetValue(ViewPropertyProperty, value); }
        }

        #endregion
    }
}