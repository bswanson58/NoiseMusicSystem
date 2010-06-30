#region Using Directives

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using Composite.Layout.Exceptions;
using Composite.Layout.Extensions;
using Composite.Layout.Properties;

#endregion

namespace Composite.Layout.Configuration
{
    public class ConfigLayoutProvider : LayoutProviderBase
    {
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config.ContainsKey("InnerXml"))
            {
                var xml = config["InnerXml"];
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                CreateLayoutManager(xmlDocument.DocumentElement);
            }

            if (LayoutManager == null)
            {
                throw new NullReferenceException(Resources.NullLayoutManagerErrorMessage);
            }

            IsInitialized = true;
        }

        private void CreateLayoutManager(XmlNode section)
        {
            LayoutManager = new LayoutManager();

            foreach (XmlAttribute attrib in section.Attributes)
            {
                switch (attrib.Name.ToLower())
                {
                    case "shellname":
                        LayoutManager.ShellName = attrib.Value;
                        break;
                }
            }

            foreach (XmlNode node in section.ChildNodes[0].ChildNodes)
            {
                //<layout>
                var layout = new Layout();

                foreach (XmlAttribute attrib in node.Attributes)
                {
                    switch (attrib.Name.ToLower())
                    {
                        case "name":
                            layout.Name = attrib.Value;
                            break;
                        case "filename":
                            layout.Filename = attrib.Value;
                            break;
                        case "fullname":
                            layout.Fullname = attrib.Value;
                            break;
                        case "description":
                            layout.Description = attrib.Value;
                            break;
                        case "isdefault":
                            if (!string.IsNullOrEmpty(attrib.Value))
                            {
                                layout.IsDefault = bool.Parse(attrib.Value);
                            }
                            break;
                        case "typename":
                            layout.TypeName = attrib.Value;
                            break;
                        case "thumbnailsource":
                            if (!string.IsNullOrEmpty(attrib.Value))
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(attrib.Value, UriKind.RelativeOrAbsolute);
                                bitmap.EndInit();
                                layout.ThumbnailSource = bitmap;
                            }
                            break;
                    }
                }

                ProcessViews(node, layout);
                LayoutManager.Layouts.Add(layout);
            }

            if (LayoutManager.Layouts.Count == 0)
            {
                throw new EmptyLayoutsCollectionException();
            }
        }

        private static void ProcessViews(XmlNode node, ILayout layout)
        {
            foreach (XmlNode viewsNode in node.ChildNodes)
            {
                if (string.Equals(viewsNode.Name, "views", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (XmlNode viewNode in viewsNode.ChildNodes)
                    {
                        if (string.Equals(viewNode.Name, "view", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var view = ProcessViewNode(viewNode);
                            layout.Views.Add(view);
                        }
                        else if (string.Equals(viewNode.Name, "viewmodel", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //its a ViewModel
                            var viewModel = ProcessViewModelNode(viewNode);
                            layout.Views.Add(viewModel);
                        }
                    }
                }
            }
        }

        private static IViewModel ProcessViewModelNode(XmlNode viewNode)
        {
            var viewModel = new ViewModel();

            foreach (XmlAttribute viewAttribute in viewNode.Attributes)
            {
                switch (viewAttribute.Name.ToLower())
                {
                    case "typename":
                        viewModel.TypeName = viewAttribute.Value;
                        break;
                    case "regionname":
                        viewModel.RegionName = viewAttribute.Value;
                        break;
                    case "visibility":
                        viewModel.Visibility = (Visibility) Enum.Parse(typeof (Visibility), viewAttribute.Value);
                        break;
                    case "viewproperty":
                        viewModel.ViewProperty = viewAttribute.Value;
                        break;
                }
            }
            return viewModel;
        }

        private static IView ProcessViewNode(XmlNode viewNode)
        {
            var view = new View();

            foreach (XmlAttribute viewAttribute in viewNode.Attributes)
            {
                switch (viewAttribute.Name.ToLower())
                {
                    case "typename":
                        view.TypeName = viewAttribute.Value;
                        break;
                    case "regionname":
                        view.RegionName = viewAttribute.Value;
                        break;
                    case "visibility":
                        view.Visibility = (Visibility) Enum.Parse(typeof (Visibility), viewAttribute.Value);
                        break;
                }
            }
            return view;
        }
    }
}