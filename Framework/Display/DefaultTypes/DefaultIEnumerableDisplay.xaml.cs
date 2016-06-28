using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

namespace Framework.Display.DefaultTypes
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class DefaultIEnumerableDisplay : UserControl, IDataDisplayable
    {
        private Dictionary<Expander, object> openedObjects;

        public void DisplayData(Binding dataBinding, string propertyName)
        {
            openedObjects = new Dictionary<Expander, object>();
            InitializeComponent();
            //listview.TargetUpdated += Listview_SourceUpdated;
            dataBinding.NotifyOnTargetUpdated = true;
            listview.SetBinding(ListView.ItemsSourceProperty, dataBinding);
            this.displayName.Text = propertyName;
        }

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            var expander = (Expander)sender;
            expander.Content = Generator.CreateStackPanelForObject(expander.DataContext);
        }
    }
}
