using System.Windows.Media;
using System.Windows;

namespace RevitNinja.Utils
{
    public static class VisualUtils
    {
        public static T FindVisualParent<T>(FrameworkElement element, string name) where T : FrameworkElement
        {
            var parentElement = (FrameworkElement)VisualTreeHelper.GetParent(element);
            while (parentElement != null)
            {
                if (parentElement is T parent)
                    if (parentElement.Name == name)
                        return parent;

                parentElement = (FrameworkElement)VisualTreeHelper.GetParent(parentElement);
            }

            return null;
        }

        public static T FindVisualChild<T>(FrameworkElement element, string name) where T : Visual
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var childElement = (FrameworkElement)VisualTreeHelper.GetChild(element, i);
                if (childElement is T child)
                    if (childElement.Name == name)
                        return child;

                var descendent = FindVisualChild<T>(childElement, name);
                if (descendent != null) return descendent;
            }

            return null;
        }
        public static List<T> FindVisualChildren<T>(FrameworkElement element) where T : Visual
        {
            List<T> result = new List<T>();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var childElement = (FrameworkElement)VisualTreeHelper.GetChild(element, i);
                if (childElement is T child)
                    result.Add(child);

                var descendent = FindVisualChildren<T>(childElement);
                if (descendent != null) result.AddRange(descendent);
            }

            return result;
        }
    }
}
