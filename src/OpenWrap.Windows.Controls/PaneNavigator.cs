using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenWrap.Windows.Controls
{
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof (ScrollViewer))]
    public class PaneNavigator : Selector
    {
        public static readonly DependencyPropertyKey PaneWidthPropertyKey;
        public static readonly DependencyProperty VisibleItemsProperty;
        public static readonly DependencyProperty MinimumItemsProperty;
        public static readonly DependencyPropertyKey IsCurrentPanePropertyKey;
        private int? _previousSelected;


        static PaneNavigator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PaneNavigator),
                                                     new FrameworkPropertyMetadata(typeof (PaneNavigator)));
            PaneWidthPropertyKey = DependencyProperty.RegisterReadOnly("PaneWidth", typeof (double),
                                                                       typeof (PaneNavigator),
                                                                       new FrameworkPropertyMetadata());
            VisibleItemsProperty = DependencyProperty.Register("VisibleItems", typeof (int), typeof (PaneNavigator),
                                                               new FrameworkPropertyMetadata(2,
                                                                                             FrameworkPropertyMetadataOptions
                                                                                                 .AffectsRender));
            MinimumItemsProperty = DependencyProperty.Register("MinimumItems", typeof (int), typeof (PaneNavigator),
                                                               new FrameworkPropertyMetadata(1));
            IsCurrentPanePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsCurrentPane", typeof (bool),
                                                                           typeof (PaneNavigator),
                                                                           new FrameworkPropertyMetadata(false,
                                                                                                         FrameworkPropertyMetadataOptions
                                                                                                             .Inherits));
        }

        public PaneNavigator()
        {
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, HandleBrowseBackCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, HandleGoToPageCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, HandleBrowseForwardCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseHome, HandleBrowseHomeCommand));

            NavigateToPaneIndex(0);
        }

        public static void SetIsCurrentPane(UIElement element, bool value)
        {
            element.SetValue(IsCurrentPanePropertyKey, value);
        }

        private void NavigateToPaneIndex(int index)
        {
            SelectedIndex = index;
            if (SelectedItem == null) return;
            if (_previousSelected != null)
            {
                var uiElement = ItemContainerGenerator.ContainerFromIndex(_previousSelected.Value) as UIElement;
                uiElement.Focusable = false;
                SetIsCurrentPane(uiElement, false);
            }

            var newPane = ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
            newPane.Focusable = true;
            SetIsCurrentPane(newPane, true);
            _previousSelected = index;
        }

        public double PaneWidth
        {
            get { return (double) GetValue(PaneWidthPropertyKey.DependencyProperty); }
        }

        public int MinimumItems
        {
            get { return (int) GetValue(MinimumItemsProperty); }
            set { SetValue(MinimumItemsProperty, value); }
        }

        public int VisibleItems
        {
            get { return (int) GetValue(VisibleItemsProperty); }
            set { SetValue(VisibleItemsProperty, value); }
        }

        private void HandleBrowseHomeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateToPaneIndex(0);

            e.Handled = true;
        }

        private void HandleBrowseForwardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var collection = ItemsSource as IList;
            if (collection == null) return;
            if (SelectedIndex < collection.Count - 1)
                NavigateToPaneIndex(SelectedIndex+1);
            ScrollToSelected();
            e.Handled = true;
        }

        private void HandleGoToPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var collection = ItemsSource as IList;

            if (collection == null) return;

            var currentIndex = FindItemSourceIndexAsDataContextFromParents(collection, e.OriginalSource);

            if (currentIndex != -1)
                while (collection.Count > currentIndex + 1)
                    collection.RemoveAt(collection.Count - 1);

            collection.Add(e.Parameter);
            NavigateToPaneIndex(collection.Count - 1);
            ScrollToSelected();
            e.Handled = true;
        }

        private int FindItemSourceIndexAsDataContextFromParents(IList collection, object source)
        {
            var fe = source as FrameworkElement;
            var fce = source as FrameworkContentElement;
            if (fe == null && fce == null) return -1;

            var dataContext = fe != null ? fe.DataContext : fce.DataContext;
            if (collection.Contains(dataContext))
                return collection.IndexOf(dataContext);
            return -1;
        }

        private void HandleBrowseBackCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var collection = ItemsSource as IList;
            if (collection == null || collection.Count <= MinimumItems)
                return;
            if (SelectedIndex > 0)
                NavigateToPaneIndex(SelectedIndex-1);
            ScrollToSelected();
            e.Handled = true;
        }

        private void ScrollToSelected()
        {
            var containerToNavigateTo = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

            var fe = containerToNavigateTo as UIElement;

            var viewer = Template != null ? Template.FindName("PART_ScrollViewer", this) as ScrollViewer : null;
            if (viewer == null) return;

            if (fe != null)
            {
                fe.Focus();
            }
            viewer.ScrollToHorizontalOffset(Math.Max(0, SelectedIndex - 1)*PaneWidth);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var arrangedSize = base.ArrangeOverride(arrangeBounds);
            SetValue(PaneWidthPropertyKey, arrangeBounds.Width/VisibleItems);
            return arrangedSize;
        }
    }
    public class PaneItem : Panel
    {
        static PaneItem()
        {
            
        }
    }
}