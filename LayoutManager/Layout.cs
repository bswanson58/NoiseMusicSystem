#region Using Directives

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

#endregion

namespace Composite.Layout
{
    [RuntimeNameProperty("Name")]
    public class Layout : DependencyObject, ILayout
    {
        public static readonly DependencyProperty ContentControlProperty = DependencyProperty.Register(
            "ContentControl", typeof (ContentControl), typeof (Layout), null);

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof (string), typeof (Layout), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
            "Filename", typeof (string), typeof (Layout), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FullnameProperty = DependencyProperty.Register(
            "Fullname", typeof (string), typeof (Layout), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsDefaultProperty = DependencyProperty.Register(
            "IsDefault", typeof (bool), typeof (Layout), new PropertyMetadata(false));

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name", typeof (string), typeof (Layout), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ThumbnailSourceProperty = DependencyProperty.Register(
            "ThumbnailSource", typeof (ImageSource), typeof (Layout), null);

        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register(
            "TypeName", typeof (string), typeof (Layout), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof (Type), typeof (Layout), null);

        public static readonly DependencyProperty ViewsProperty = DependencyProperty.Register(
            "Views", typeof (List<IView>), typeof (Layout), new PropertyMetadata(new List<IView>()));

        public Layout()
        {
            Views = new List<IView>();
        }

        #region ILayout Members

        public string Description
        {
            get { return (string) GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public ImageSource ThumbnailSource
        {
            get { return (ImageSource) GetValue(ThumbnailSourceProperty); }
            set { SetValue(ThumbnailSourceProperty, value); }
        }

        public string Filename
        {
            get { return (string) GetValue(FilenameProperty); }
            set { SetValue(FilenameProperty, value); }
        }

        public string Fullname
        {
            get { return (string) GetValue(FullnameProperty); }
            set { SetValue(FullnameProperty, value); }
        }

        public string Name
        {
            get { return (string) GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string TypeName
        {
            get { return (string) GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public bool IsDefault
        {
            get { return (bool) GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        public Type Type
        {
            get { return (Type) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public ContentControl ContentControl
        {
            get { return (ContentControl) GetValue(ContentControlProperty); }
            set { SetValue(ContentControlProperty, value); }
        }

        public List<IView> Views
        {
            get { return (List<IView>) GetValue(ViewsProperty); }
            set { SetValue(ViewsProperty, value); }
        }

        #endregion
    }
}