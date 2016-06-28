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
    public class DefaultDateTimeDisplay : StackPanel, IDataDisplayable
    {
        public void DisplayData(Binding dataBinding, string propertyName)
        {
            this.Orientation = Orientation.Horizontal;

            var valueDisplay = new DatePicker();
            valueDisplay.Margin = Preferences.DefaultMargin;
            valueDisplay.Height = Math.Max(Preferences.DefaultValueHeight, 25);
            valueDisplay.Width = Preferences.DefaultValueWidth;

            valueDisplay.SetBinding(DatePicker.SelectedDateProperty, dataBinding);

            var nameDisplay = new TextBlock();
            nameDisplay.VerticalAlignment = VerticalAlignment.Center;
            nameDisplay.Width = Preferences.DefaultLabelWidth;
            nameDisplay.TextWrapping = TextWrapping.Wrap;
            nameDisplay.FontWeight = FontWeights.Bold;
            nameDisplay.Text = propertyName;
            nameDisplay.Margin = Preferences.DefaultMargin;

            this.Children.Add(nameDisplay);
            this.Children.Add(valueDisplay);
        }
    }
}
