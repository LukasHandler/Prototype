using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections;
using Framework.Display;

namespace Framework
{
    public partial class GeneratedUserControl : UserControl
    {
        public object GenerationSource
        {
            get { return (object)GetValue(GenerationSourceProperty); }
            set { SetValue(GenerationSourceProperty, value); }
        }

        public static readonly DependencyProperty GenerationSourceProperty =
            DependencyProperty.Register("GenerationSource", typeof(object), typeof(GeneratedUserControl));

        public GeneratedUserControl()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void StartGenerationProcess(object sender, RoutedEventArgs e)
        {
            this.content.Children.Add(Generator.CreateStackPanelForObject(this.GenerationSource));
        }
    }
}
