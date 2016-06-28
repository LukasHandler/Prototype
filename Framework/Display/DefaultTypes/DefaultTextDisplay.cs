using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Framework.Display.DefaultTypes
{
    public class DefaultTextDisplay : StackPanel, IDataDisplayable
    {
        public void DisplayData(Binding dataBinding, string propertyName)
        {
            this.Orientation = Orientation.Horizontal;

            var nameDisplay = new TextBlock();
            nameDisplay.VerticalAlignment = VerticalAlignment.Center;
            nameDisplay.Width = Preferences.DefaultLabelWidth;
            nameDisplay.TextWrapping = TextWrapping.Wrap;
            nameDisplay.Text = propertyName;
            nameDisplay.FontWeight = FontWeights.Bold;
            nameDisplay.Margin = Preferences.DefaultMargin;

            var valueDisplay = new TextBox();
            valueDisplay.Margin = Preferences.DefaultMargin;
            valueDisplay.Height = Preferences.DefaultValueHeight;
            valueDisplay.Width = Preferences.DefaultValueWidth;
            valueDisplay.SetBinding(TextBox.TextProperty, dataBinding);

            this.Children.Add(nameDisplay);
            this.Children.Add(valueDisplay);
        }
    }
}
