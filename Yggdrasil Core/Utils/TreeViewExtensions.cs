using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yggdrasil_Core.Utils
{
    public static class TreeViewExtensions
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewExtensions),
                new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        public static object GetSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
                treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;

                if (e.NewValue != null && e.NewValue != e.OldValue)
                {
                    SelectItem(treeView, e.NewValue);
                }
            }
        }

        private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                SetSelectedItem(treeView, e.NewValue);
            }
        }

        private static void SelectItem(TreeView treeView, object item)
        {
            if (item == null) return;
            treeView.UpdateLayout();
            var tvi = GetTreeViewItem(treeView, item);
            if (tvi != null)
            {
                tvi.IsSelected = true;
                tvi.BringIntoView();
            }
        }

        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container == null) return null;

            if (container.DataContext == item)
            {
                return container as TreeViewItem;
            }

            if (container is TreeViewItem tvi && !tvi.IsExpanded)
            {
                tvi.IsExpanded = true;
                container.UpdateLayout();
            }

            for (int i = 0; i < container.Items.Count; i++)
            {
                var subContainer = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (subContainer == null) continue;
                var result = GetTreeViewItem(subContainer, item);
                if (result != null) return result;
            }

            return null;
        }
    }
}