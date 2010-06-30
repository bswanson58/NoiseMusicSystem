using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Composite.Layout.Events;
using Composite.Layout.Exceptions;
using Composite.Layout.Properties;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace Composite.Layout
{
    public class LayoutManager : DependencyObject, ILayoutManager, INotifyPropertyChanged
    {
        public static readonly DependencyProperty LayoutsProperty = DependencyProperty.Register(
            "Layouts", typeof (List<ILayout>), typeof (LayoutManager), new PropertyMetadata(new List<ILayout>()));

        public static readonly DependencyProperty ShellNameProperty = DependencyProperty.Register(
            "ShellName", typeof (string), typeof (LayoutManager), new PropertyMetadata(string.Empty));

        private IUnityContainer _Container;
        private ILayout _CurrentLayout;
        private bool _IsInitialized;
        private List<ILayout> _Layouts;

        /// <summary>
        /// Internal constructor used by the LayoutConfigurationSection to create the LayoutManager from a config file
        /// </summary>
        public LayoutManager()
        {
            _Layouts = new List<ILayout>();
        }

        /// <summary>
        /// Public constructor intended for use when not using a config file to define layouts
        /// </summary>
        /// <param name="container">An instance of the UnityContainer</param>
        /// <param name="layouts">A collection of ILayout objects</param>
        public LayoutManager(IUnityContainer container, List<ILayout> layouts)
        {
            if (container == null)
            {
                throw new NullReferenceException("container must not be null");
            }

            if (layouts == null)
            {
                throw new NullReferenceException("layouts must not be null");
            }

            if (layouts.Count == 0)
            {
                throw new EmptyLayoutsCollectionException();
            }

            _Container = container;
            _Layouts = layouts;
            Initialize(_Container);
        }

        #region ILayoutManager Members

        /// <summary>
        /// Gets or sets the region name contained in the application Shell
        /// </summary>
        public string ShellName
        {
            get { return (string) GetValue(ShellNameProperty); }
            set
            {
                SetValue(ShellNameProperty, value);
                OnPropertyChanged("ShellName");
            }
        }

        /// <summary>
        /// Gets or sets a collection of ILayout objects which define the layouts and views used by the application
        /// </summary>
        public List<ILayout> Layouts
        {
            get { return (List<ILayout>) GetValue(LayoutsProperty); }
            set
            {
                SetValue(LayoutsProperty, value);
                OnPropertyChanged("Layouts");
            }
        }

        /// <summary>
        /// Gets the currently loaded layout
        /// </summary>
        public ILayout CurrentLayout
        {
            get { return _CurrentLayout; }
            private set
            {
                if (_CurrentLayout != value)
                {
                    _CurrentLayout = value;
                    OnPropertyChanged("CurrentLayout");
                }
            }
        }

        /// <summary>
        /// Loads the default layout into the Shell region
        /// </summary>
        public void LoadLayout()
        {
            if (string.IsNullOrEmpty(ShellName))
            {
                throw new NullReferenceException(Resources.NullShellNameErrorMessage);
            }

            var defaultLayout = GetDefaultLayout();

            if (defaultLayout == null)
            {
                throw new NullReferenceException(Resources.NoDefaultLayoutErrorMessage);
            }

            LoadLayout(ShellName, defaultLayout.Name);
        }

        /// <summary>
        /// Loads a layout by name into the Shell
        /// </summary>
        /// <param name="layoutName"></param>
        public void LoadLayout(string layoutName)
        {
            LoadLayout(ShellName, layoutName);
        }


        /// <summary>
        /// Initializes the container and layout objects
        /// </summary>
        /// <param name="container">An instance of the UnityContainer</param>
        public void Initialize(IUnityContainer container)
        {
            _Container = container;

            foreach (var layout in Layouts)
            {
                LoadLayoutContentControl(layout);
            }

            _IsInitialized = true;

            var eventAggregator = _Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<LayoutManagerInitializedEvent>().Publish(null);
        }

        public bool IsInitialized
        {
            get { return _IsInitialized; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void LoadLayoutContentControl(ILayout layout)
        {
            if (layout.ContentControl == null)
            {
                if (layout.Type != null)
                {
                    layout.ContentControl = _Container.Resolve(layout.Type) as ContentControl;
                }
                else if (string.IsNullOrEmpty(layout.Filename) && !string.IsNullOrEmpty(layout.TypeName))
                {
                    //type declaration
                    layout.Type = Type.GetType(layout.TypeName);
                    layout.ContentControl = _Container.Resolve(layout.Type) as ContentControl;
                }
                else if (!string.IsNullOrEmpty(layout.Filename))
                {
                    string path;

                    //loose Xaml 
                    if (File.Exists(layout.Filename))
                    {
                        path = layout.Filename;
                    }
                    else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, layout.Filename)))
                    {
                        path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, layout.Filename);
                    }
                    else
                    {
                        throw new FileNotFoundException(string.Format(Resources.LayoutFileNotFoundErrorMessage, layout.Filename));
                    }

                    layout.ContentControl = XamlReader.Load(File.OpenRead(path)) as ContentControl;
                }
                else
                {
                    throw new LayoutConfigurationException();
                }
            }
        }

        /// <summary>
        /// Loads a layout by name into a named region
        /// </summary>
        /// <param name="regionName">The name of the target region</param>
        /// <param name="layoutName">The name of the layout to load</param>
        private void LoadLayout(string regionName, string layoutName)
        {
            if (!IsInitialized)
            {
                throw new NotInitializedException();
            }

            ILayout layout = null;
            foreach (ILayout layoutObject in Layouts)
            {
                if (layoutObject.Name == layoutName)
                {
                    layout = layoutObject;
                    break;
                }
            }

            if (layout == null)
            {
                throw new LayoutNotFoundException(layoutName);
            }

            LoadLayoutContentControl(layout);

            var eventAggregator = _Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<LayoutLoadingEvent>().Publish(layout);

            var regionManager = _Container.Resolve<IRegionManager>();

            //check to see if we have a current layout, if so, remove it
            if (CurrentLayout != null)
            {
                eventAggregator.GetEvent<LayoutUnloadingEvent>().Publish(CurrentLayout);
                //remove all the views that have been placed in the layout's regions
                RemoveViews(regionManager, CurrentLayout);
                //remove the layout from the region
                RemoveLayoutControl(regionManager.Regions[regionName], CurrentLayout);
                eventAggregator.GetEvent<LayoutUnloadedEvent>().Publish(CurrentLayout);
            }

            //add the new layout to the parent region
            regionManager.Regions[regionName].Add(layout.ContentControl, layout.Name);

            var userControl = layout.ContentControl;

            if (userControl != null)
            {
                //register the regions contained within the layout UserControl
                RegisterRegions(userControl.Content as DependencyObject, regionManager);
                RegionManager.UpdateRegions();
                //load the views that have been targeted for this layout
                LoadViews(layout, regionManager);
                CurrentLayout = layout;
            }
            else
            {
                throw new LayoutControlNotUserControlException();
            }

            eventAggregator.GetEvent<LayoutLoadedEvent>().Publish(layout);
        }

        /// <summary>
        /// Remove the layout control from its region
        /// </summary>
        /// <param name="region">The region that contains the layout</param>
        /// <param name="layout">The layout control you want to remove</param>
        private static void RemoveLayoutControl(IRegion region, ILayout layout)
        {
            var regionType = region.GetType();
            var property = regionType.GetProperty("ItemMetadataCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            var itemDataCollection = property.GetValue(region, null) as ObservableCollection<ItemMetadata>;

            if (itemDataCollection != null)
            {
                ItemMetadata selectedMetaData = null;

                foreach (ItemMetadata metaData in itemDataCollection)
                {
                    if (metaData.Item == layout.ContentControl)
                    {
                        selectedMetaData = metaData;
                        break;
                    }
                }

                if (selectedMetaData != null)
                {
                    itemDataCollection.Remove(selectedMetaData);
                }
            }
        }

        /// <summary>
        /// Removes the views that have been placed in the layout's Regions
        /// </summary>
        /// <param name="regionManager">An instance of IRegionManager</param>
        /// <param name="layout">An instance of ILayout that holds the views</param>
        private void RemoveViews(IRegionManager regionManager, ILayout layout)
        {
            if (CurrentLayout != null && CurrentLayout.Views != null && CurrentLayout.Views.Count > 0)
            {
                foreach (var view in CurrentLayout.Views)
                {
                    if (view is IViewModel)
                    {
                        var viewModel = _Container.Resolve(view.Type);
                        var viewProperty = viewModel.GetType().GetProperty(((ViewModel) view).ViewProperty);
                        var viewModelView = viewProperty.GetValue(viewModel, null);
                        regionManager.Regions[view.RegionName].Remove(viewModelView);
                    }
                    else
                    {
                        var viewControl = _Container.Resolve(view.Type);
                        regionManager.Regions[view.RegionName].Remove(viewControl);
                    }

                    regionManager.Regions.Remove(view.RegionName);
                }

                ClearRegions(layout.ContentControl);
            }
        }

        /// <summary>
        /// Recursively clears out any controls which are currently bound to a region
        /// </summary>
        /// <param name="dependencyObject">The DependecyObject which contains the region(s)</param>
        private static void ClearRegions(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new NullReferenceException(Resources.NullDependencyObjectErrorMessage);
            }

            var childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

            for (var childIndex = 0; childIndex < childCount; childIndex++)
            {
                var childObject = VisualTreeHelper.GetChild(dependencyObject, childIndex);

                var name = RegionManager.GetRegionName(childObject);

                if (!string.IsNullOrEmpty(name))
                {
                    //okay, we have a control that has a region defined in it, so we need to clear out its contents
                    if (childObject is ItemsControl)
                    {
                        ((ItemsControl) childObject).ItemsSource = null;
                    }
                    else if (childObject is ContentControl)
                    {
                        ((ContentControl) childObject).Content = null;
                    }
                    else if (childObject is Panel)
                    {
                        ((Panel) childObject).Children.Clear();
                    }
                }

                ClearRegions(childObject);
            }
        }

        /// <summary>
        /// Recursively finds regions that need to be registered with the RegionManager
        /// </summary>
        /// <param name="dependencyObject">The DependecyObject which contains the region(s)</param>
        /// <param name="regionManager">An instance of the RegionManager</param>
        private static void RegisterRegions(DependencyObject dependencyObject, IRegionManager regionManager)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

            for (var childIndex = 0; childIndex < childCount; childIndex++)
            {
                var childObject = VisualTreeHelper.GetChild(dependencyObject, childIndex);

                var name = RegionManager.GetRegionName(childObject);

                if (!string.IsNullOrEmpty(name) &&
                    !regionManager.Regions.ContainsRegionWithName(name))
                {
                    var locator = ServiceLocator.Current;
                    var regionCreationBehavior = locator.GetInstance<DelayedRegionCreationBehavior>();
                    regionCreationBehavior.TargetElement = childObject;
                    regionCreationBehavior.Attach();
                }

                RegisterRegions(childObject, regionManager);
            }
        }


        /// <summary>
        /// Loads the views defined by a Layout into RegionManager
        /// </summary>
        /// <param name="layout">An instance of the Layout</param>
        /// <param name="regionManager">An instance of the RegionManager</param>
        private void LoadViews(ILayout layout, IRegionManager regionManager)
        {
            foreach (var view in layout.Views)
            {
                if (view.Type == null)
                {
                    view.Type = Type.GetType(view.TypeName);

                    if (view.Type == null)
                    {
                        throw new NullReferenceException(string.Format(Resources.NullViewTypeErrorMessage, view.TypeName));
                    }
                }

                object viewToLoad;

                if (view is IViewModel)
                {
                    var viewModel = _Container.Resolve(view.Type);
                    var viewProperty = viewModel.GetType().GetProperty(((ViewModel) view).ViewProperty);
                    viewToLoad = viewProperty.GetValue(viewModel, null);
                }
                else
                {
                    viewToLoad = _Container.Resolve(view.Type);
                }

                if (viewToLoad != null)
                {
                    regionManager.Regions[view.RegionName].Add(viewToLoad);
                }

                if (viewToLoad is UIElement)
                {
                    ((UIElement) viewToLoad).Visibility = view.Visibility;
                }
            }
        }

        /// <summary>
        /// Returns the default Layout, as set by the IsDefault property
        /// </summary>
        /// <returns></returns>
        public ILayout GetDefaultLayout()
        {
            return Layouts.Single(c => c.IsDefault.Equals(true));
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}