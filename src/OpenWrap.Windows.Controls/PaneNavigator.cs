using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenWrap.Windows.Controls
{
    public delegate void RoutedEventHandler<T>(object source, T eventArgs) where T : RoutedEventArgs;
    public class PaneScrollStartEventArgs : RoutedEventArgs
    {
        public PaneScrollStartEventArgs(object source, RoutedEvent routedEvent, double sourceScrollOffset, double destinationScrollOffset)
            : base(routedEvent, source)
        {
            SourceScrollOffset = sourceScrollOffset;
            DestinationScrollOffset = destinationScrollOffset;
        }

        public double SourceScrollOffset { get; set; }
        public double DestinationScrollOffset { get; set; }
    }
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    public class PaneNavigator : Selector
    {
        public static readonly DependencyPropertyKey PaneWidthPropertyKey;
        public static readonly DependencyProperty VisibleItemsProperty;
        public static readonly DependencyProperty MinimumItemsProperty;
        public static readonly DependencyPropertyKey IsCurrentPanePropertyKey;
        public static readonly DependencyProperty IsCurrentPaneProperty;
        private int? _previousSelected;
        public static readonly RoutedEvent PaneScrollStartEvent;
        private static DependencyProperty HorizontalScrollIsAttachedProperty;
        private static DependencyProperty HorizontalScrollOffsetProperty;
        public static readonly DependencyProperty FirstPaneProperty;


        static PaneNavigator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneNavigator),
                                                     new FrameworkPropertyMetadata(typeof(PaneNavigator)));
            PaneWidthPropertyKey = DependencyProperty.RegisterReadOnly("PaneWidth", typeof(double),
                                                                       typeof(PaneNavigator),
                                                                       new FrameworkPropertyMetadata());
            VisibleItemsProperty = DependencyProperty.Register("VisibleItems", typeof(int), typeof(PaneNavigator),
                                                               new FrameworkPropertyMetadata(2,
                                                                                             FrameworkPropertyMetadataOptions
                                                                                                 .AffectsRender));
            MinimumItemsProperty = DependencyProperty.Register("MinimumItems", typeof(int), typeof(PaneNavigator),
                                                               new FrameworkPropertyMetadata(1));
            IsCurrentPanePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsCurrentPane", typeof(bool),
                                                                           typeof(PaneNavigator),
                                                                           new FrameworkPropertyMetadata(false,
                                                                                                         FrameworkPropertyMetadataOptions
                                                                                                             .Inherits));
            HorizontalScrollIsAttachedProperty = DependencyProperty.RegisterAttached("HorizontalScrollIsAttached", typeof(bool),
                                                                             typeof(PaneNavigator),
                                                                             new FrameworkPropertyMetadata(false, HorizontalScrollIsAttachedChanged));
            HorizontalScrollOffsetProperty = DependencyProperty.RegisterAttached("HorizontalScrollOffset",
                                                                                 typeof(double), typeof(PaneNavigator),
                                                                                 new FrameworkPropertyMetadata((double)0,
                                                                                                               FrameworkPropertyMetadataOptions
                                                                                                                   .
                                                                                                                   AffectsArrange |
                                                                                                               FrameworkPropertyMetadataOptions
                                                                                                                   .
                                                                                                                   AffectsMeasure,
                                                                                                               HorizontalScrollChanged));
            IsCurrentPaneProperty = IsCurrentPanePropertyKey.DependencyProperty;
            PaneScrollStartEvent = EventManager.RegisterRoutedEvent("PaneScrollStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler<PaneScrollStartEventArgs>),
                                             typeof(PaneNavigator));

            FirstPaneProperty = DependencyProperty.Register("FirstPane", typeof (object), typeof (PaneNavigator),
                                                            new PropertyMetadata(HandleFirstPaneChanged));

        }

        private static void HandleFirstPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaneNavigator)d).ItemsSource = new ObservableCollection<object>{e.NewValue};
        }
        public object FirstPane
        {
            get { return GetValue(FirstPaneProperty); }
            set{ SetValue(FirstPaneProperty, value);
            }
        }
        public event RoutedEventHandler<PaneScrollStartEventArgs> PaneScrollStart
        {
            add { AddHandler(PaneScrollStartEvent, value); }
            remove { RemoveHandler(PaneScrollStartEvent, value); }
        }
        protected virtual void RaisePaneScrollStart(double from, double to)
        {
            RaiseEvent(new PaneScrollStartEventArgs(this, PaneScrollStartEvent, from, to));
        }
        public static bool GetHorizontalScrollIsAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(HorizontalScrollIsAttachedProperty);
        }
        public static void SetHorizontalScrollIsAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(HorizontalScrollIsAttachedProperty, value);
        }
        public static double GetHorizontalScrollOffset(DependencyObject obj)
        {
            return (double) obj.GetValue(HorizontalScrollOffsetProperty);
        }
        public static void SetHorizontalScrollOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalScrollOffsetProperty, value);
        }
        private static void HorizontalScrollIsAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs property)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null) return;
            if ((bool)property.NewValue == true)
                scrollViewer.ScrollChanged += (s, e) => SetHorizontalScrollOffset(scrollViewer, scrollViewer.ContentHorizontalOffset);
        }
        private static void HorizontalScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var target = d as ScrollViewer;
            if (target == null) return;
            target.ScrollToHorizontalOffset((double)dependencyPropertyChangedEventArgs.NewValue);
        }

        public PaneNavigator()
        {
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, HandleBrowseBackCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, HandleGoToPageCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, HandleBrowseForwardCommand));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseHome, HandleBrowseHomeCommand));

            NavigateToPaneIndex(0);
        }
        public static bool GetIsCurrentPane(UIElement element)
        {
            return (bool)element.GetValue(IsCurrentPaneProperty);
        }
        static void SetIsCurrentPane(UIElement element, bool value)
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
                if (uiElement != null)
                {
                    uiElement.Focusable = false;
                    SetIsCurrentPane(uiElement, false);
                }
            }

            var newPane = ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
            if (newPane != null)
            {
                newPane.Focusable = true;
                SetIsCurrentPane(newPane, true);
            }
            _previousSelected = index;
        }

        public double PaneWidth
        {
            get { return (double)GetValue(PaneWidthPropertyKey.DependencyProperty); }
        }

        public int MinimumItems
        {
            get { return (int)GetValue(MinimumItemsProperty); }
            set { SetValue(MinimumItemsProperty, value); }
        }

        public int VisibleItems
        {
            get { return (int)GetValue(VisibleItemsProperty); }
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
                NavigateToPaneIndex(SelectedIndex + 1);
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
                NavigateToPaneIndex(SelectedIndex - 1);
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
            var to = Math.Max(0, SelectedIndex - 1) * PaneWidth;
            RaisePaneScrollStart(GetHorizontalScrollOffset(viewer), to); 
            SetHorizontalScrollOffset(viewer, to);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var arrangedSize = base.ArrangeOverride(arrangeBounds);

            SetValue(PaneWidthPropertyKey, arrangeBounds.Width / VisibleItems);
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