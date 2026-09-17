using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting.Helpers
{
    internal class TreeViewItemBehaviors
    {
        public static readonly DependencyProperty SelectOnRightClickProperty =
           DependencyProperty.RegisterAttached(
               "SelectOnRightClick",
               typeof(bool),
               typeof(TreeViewItemBehaviors),
               new PropertyMetadata(false, OnSelectOnRightClickChanged));

        public static bool GetSelectOnRightClick(DependencyObject obj) =>
            (bool)obj.GetValue(SelectOnRightClickProperty);

        public static void SetSelectOnRightClick(DependencyObject obj, bool value) =>
            obj.SetValue(SelectOnRightClickProperty, value);

        private static void OnSelectOnRightClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem tvi)
            {
                if ((bool)e.NewValue)
                {
                    tvi.PreviewMouseRightButtonDown += Tvi_PreviewMouseRightButtonDown;
                }
                else
                {
                    tvi.PreviewMouseRightButtonDown -= Tvi_PreviewMouseRightButtonDown;
                }
            }
        }

        private static void Tvi_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem tvi)
            {
                tvi.IsSelected = true;
                e.Handled = false; // allow ContextMenu to open
            }
        }

    }
}
