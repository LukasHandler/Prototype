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
    public class DefaultMethodDisplay : Button, IMethodDisplayable
    {
        public void DisplayMethod(object dataObject, MethodInfo method, Binding canExecuteBinding, string title)
        {
            this.Content = title;
            this.Width = Preferences.DefaultLabelWidth;
            this.Margin = Preferences.DefaultMargin;

            this.Click += delegate(object sender, System.Windows.RoutedEventArgs e)
            {
                Generator.GenerateMethodInputOutputDisplay(dataObject, method);
            };

            if (canExecuteBinding != null)
            {
                this.SetBinding(Button.IsEnabledProperty, canExecuteBinding);
            }
        }
    }
}
