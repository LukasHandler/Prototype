using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Framework
{
    /// <summary>
    /// This class holds default values, which are used by the default display elements.
    /// </summary>
    public static class Preferences
    {
        public static int DefaultLabelWidth = 150;
        public static int DefaultValueWidth = 150;
        public static int DefaultValueHeight = 20;
        public static Thickness DefaultMargin = new Thickness(5);
        public static int DefaultButtonWidth = 100;
        public static Brush DefaultWindowColor = Brushes.White;
    }
}
