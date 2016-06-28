using Framework.Display;
using Framework.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Framework
{
    public static class Generator
    {
        public static Configuration GeneratorConfiguration;

        public static Dictionary<object, object> Wrappers = new Dictionary<object, object>();

        public static void RefreshBindings(object instance, string propertyName)
        {
            if (Generator.Wrappers.ContainsKey(instance))
            {
                var wrapperInstance = Wrappers[instance];

                //http://stackoverflow.com/questions/30687704/fire-inotifypropertychanged-propertychanged-via-reflection
                var a = wrapperInstance.GetType().GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                var propertyChangedEventValue = (PropertyChangedEventHandler)a.GetValue(wrapperInstance);
                if (propertyChangedEventValue != null)
                {
                    propertyChangedEventValue(wrapperInstance, new PropertyChangedEventArgs(propertyName));
                }

            }
        }

        public static void CreateDataTemplateEntries(Type sourceType, Type wrapperType)
        {
            if (GeneratorConfiguration != null && GeneratorConfiguration.DataTemplates.ContainsKey(sourceType))
            {
                DataTemplate typeLayout = new DataTemplate();
                typeLayout.DataType = wrapperType;

                FrameworkElementFactory textHolder = new FrameworkElementFactory(typeof(TextBlock));
                MultiBinding displayBinding = new MultiBinding();
                int i = 0;
                string formatHolder = string.Empty;
                MultiBinding textBinding = new MultiBinding();

                foreach (var item in GeneratorConfiguration.DataTemplates[sourceType])
                {
                    var partialBinding = new Binding(item);
                    textBinding.Bindings.Add(new Binding(item));
                    formatHolder += "{" + i + "} ";
                    i++;
                }

                formatHolder.TrimEnd();
                textBinding.StringFormat = formatHolder;
                textHolder.SetBinding(TextBlock.TextProperty, textBinding);

                typeLayout.VisualTree = textHolder;
                Application.Current.Resources.Add(new DataTemplateKey(wrapperType), typeLayout);
            }
        }

        /// <summary>
        /// Generates a StackPanel from a given object instance.
        /// </summary>
        /// <param name="instance">The instance of the object, the UI should be generated from.</param>
        /// <returns>A StackPanel including the generated elements</returns>
        public static StackPanel CreateStackPanelForObject(object instance)
        {
            try
            {
                var stackPanel = new StackPanel();
                stackPanel.Margin = Preferences.DefaultMargin;
                stackPanel.Orientation = Orientation.Vertical;
                CreateUIElementForParent(instance, stackPanel);
                return stackPanel;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Bei der Generierung ist folgender Fehler aufgetreten\r\n\r\n" + e.Message);
                throw e;
            }
        }

        //http://stackoverflow.com/questions/863881/how-do-i-tell-if-a-type-is-a-simple-type-i-e-holds-a-single-value
        public static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal))
              || type.Equals(typeof(DateTime));
        }

        public static object GetWrapper(object source)
        {
            if (DOMGenerator.WrapperTypes.ContainsValue(source.GetType()))
            {
                return source;
            }

            if (Wrappers.ContainsKey(source))
            {
                return Wrappers[source];
            }
            else
            {
                var wrapperType = DOMGenerator.GetWrapperType(source.GetType());
                object concreteObject = Activator.CreateInstance(wrapperType, new object[] { source });
                Wrappers.Add(source, concreteObject);
                return concreteObject;
            }
        }

        private static void CreateUIElementForParent(object instance, StackPanel parent)
        {
            if (IsSimple(instance.GetType()))
            {
                return;
            }
            else
            {
                instance = GetWrapper(instance);
            }

            Type dataType = instance.GetType();

            var realInstance = dataType.GetField("objectInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(instance);

            foreach (PropertyInfo property in dataType.GetProperties())
            {
                var memberKey = realInstance.GetType().FullName + "." + property.Name;
                if (!GeneratorConfiguration.HidingMembers.Contains(memberKey))
                {
                    Type propType = property.PropertyType;
                    var propertyValue = property.GetValue(instance);

                    if (propType.IsGenericType && (propertyValue is IList || propertyValue is IEnumerable || propertyValue is ICollection))
                    {
                        propType = typeof(IEnumerable);
                    }
                    
                    var containsUIElement = GeneratorConfiguration.UIElements.ContainsKey(memberKey);

                    if (containsUIElement || TypeDefinitions.Dictionary.ContainsKey(propType))
                    {
                        Type uiType;

                        if (containsUIElement)
                        {
                            uiType = GeneratorConfiguration.UIElements[memberKey];

                            if (!uiType.GetInterfaces().Contains(typeof(IDataDisplayable)))
                            {
                                throw new IDataDisplayAbleNotImplementedException();
                            }

                            if (!uiType.IsSubclassOf(typeof(UIElement)))
                            {
                                throw new NotAUIElementException();
                            }
                        }
                        else
                        {
                            uiType = TypeDefinitions.Dictionary[propType];
                        }

                        var uiElement = (IDataDisplayable)Activator.CreateInstance(uiType);

                        Binding dataBinding = new Binding(property.Name);
                        dataBinding.Mode = GetBindingForProperty((UIElement)uiElement, property);
                        dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        dataBinding.Source = instance;

                        string displayName = GetTitle(instance, property.Name);

                        uiElement.DisplayData(dataBinding, displayName);

                        string uiPropertyKey = realInstance.GetType().FullName + "." + property.Name;

                        if (GeneratorConfiguration.UIPropertyOverwriteValues.ContainsKey(uiPropertyKey))
                        {
                            var overwriteValues = GeneratorConfiguration.UIPropertyOverwriteValues[uiPropertyKey];

                            foreach (var overwriteTuple in overwriteValues)
                            {
                                uiType.GetProperty(overwriteTuple.Item1).SetValue(uiElement, overwriteTuple.Item2);
                            }
                        }

                        parent.Children.Add((UIElement)uiElement);
                    }

                    else
                    {
                        propertyValue = property.GetValue(instance);

                        var propertyGroup = new GroupBox();
                        propertyGroup.Header = GetTitle(instance, property.Name);
                        parent.Children.Add(propertyGroup);

                        var propertyContent = new StackPanel();
                        propertyContent.Orientation = Orientation.Vertical;
                        propertyGroup.Content = propertyContent;

                        if (propertyValue != null)
                        {
                            if (GeneratorConfiguration.MemberConfigurators.ContainsKey(memberKey))
                            {
                                var defaultConfiguration = GeneratorConfiguration;
                                GeneratorConfiguration = GeneratorConfiguration.MemberConfigurators[memberKey];
                                CreateUIElementForParent(propertyValue, propertyContent);
                                GeneratorConfiguration = defaultConfiguration;
                            }
                            else
                            {
                                CreateUIElementForParent(propertyValue, propertyContent);
                            }
                        }
                    }
                }
            }

            foreach (var method in dataType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var hideKey = realInstance.GetType().FullName + "." + method.Name;

                //http://stackoverflow.com/questions/234330/bindingflags-for-type-getmethods-excluding-property-accesors
                if (method.IsPublic
                    // Make sure it's not a set get (Property), add and remove (Events) Method
                    && (!method.IsSpecialName
                    && (!method.Name.StartsWith("set_") && !method.Name.StartsWith("get_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove")))
                    && !GeneratorConfiguration.HidingMembers.Contains(hideKey)
                    && method.DeclaringType.FullName != "System.Object")
                {

                    var realMethod = realInstance.GetType().GetMethod(method.Name);

                    string canExecuteKey = realMethod.DeclaringType.FullName + "." + realMethod.Name;
                    PropertyInfo enableConditionProperty = null;
                    Binding canExecuteBinding = null;

                    if (GeneratorConfiguration.CanExecuteCollection.ContainsKey(canExecuteKey))
                    {
                        try
                        {
                            var function = GeneratorConfiguration.CanExecuteCollection[canExecuteKey];
                            enableConditionProperty = dataType.GetProperty(DOMGenerator.CanExecutedProperties[function]);

                            canExecuteBinding = new Binding(enableConditionProperty.Name);
                            canExecuteBinding.Mode = BindingMode.OneWay;
                            canExecuteBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                            canExecuteBinding.Source = instance;
                        }
                        catch (Exception)
                        {
                            throw new ConditionPropertyNotFoundException();
                        }
                    }

                    var uiType = TypeDefinitions.MethodDisplay;
                    var uiElement = (IMethodDisplayable)Activator.CreateInstance(uiType);

                    uiElement.DisplayMethod(instance, method, canExecuteBinding);
                    parent.Children.Add((UIElement)uiElement);
                }
            }
        }

        /// <summary>
        /// Gets the binding for a property. The BindingMode depends on the implementation of a get and set method on the property.
        /// </summary>
        /// <param name="element">The UI element to represent the property. Will be disabled if there is not set method on the property</param>
        /// <param name="property">The property the BindingMode gets created from.</param>
        /// <returns>A BindingMode which will be used for the databinding.</returns>
        private static BindingMode GetBindingForProperty(UIElement element, PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                if (Generator.IsSimple(property.PropertyType))
                {
                    element.IsEnabled = false;
                }

                return BindingMode.OneWay;
            }
            else if (!property.CanRead)
            {
                return BindingMode.OneWay;
            }
            else
            {
                return BindingMode.TwoWay;
            }

        }

        /// <summary>
        /// Generates the method's input and output elements/windows.
        /// </summary>
        /// <param name="dataObject">The object, the method should get executed on.</param>
        /// <param name="method">The method, the user wants to run.</param>
        public static void GenerateMethodInputOutputDisplay(object dataObject, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            object result;

            if (methodParameters.Count() == 0)
            {
                result = method.Invoke(dataObject, null);
                ShowResult(result);
            }
            else
            {
                Window parameterWindow = new Window();
                parameterWindow.Background = Preferences.DefaultWindowColor;
                parameterWindow.Title = "Run Method";
                parameterWindow.SizeToContent = SizeToContent.WidthAndHeight;
                StackPanel content = new StackPanel();
                content.Orientation = Orientation.Vertical;
                parameterWindow.Content = content;

                var parameterValuesObject = new { parameterValues = new object[methodParameters.Count()] };
                int i = 0;

                List<string> elementNamesWithValidatioRules = new List<string>();

                foreach (var item in methodParameters)
                {
                    var realInstance = dataObject.GetType().GetField("objectInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(dataObject);
                    var realMethod = realInstance.GetType().GetMethod(method.Name);
                    Tuple<string, string> collectionKey = new Tuple<string, string>(realMethod.DeclaringType.FullName + "." + realMethod.Name, item.Name);

                    if (GeneratorConfiguration.ParameterCollections.ContainsKey(collectionKey))
                    {
                        var possibilities = (ICollection)GeneratorConfiguration.ParameterCollections[collectionKey];

                        StackPanel selectionPanel = new StackPanel();
                        selectionPanel.Orientation = Orientation.Horizontal;

                        TextBlock displayName = new TextBlock();
                        displayName.Text = item.Name;
                        displayName.Width = Preferences.DefaultLabelWidth;
                        displayName.Margin = Preferences.DefaultMargin;
                        selectionPanel.Children.Add(displayName);

                        ComboBox valueSelection = new ComboBox();
                        valueSelection.ItemsSource = possibilities;
                        valueSelection.Width = Preferences.DefaultValueWidth;
                        valueSelection.Margin = Preferences.DefaultMargin;
                        selectionPanel.Children.Add(valueSelection);

                        foreach (var firstPos in possibilities)
                        {
                            parameterValuesObject.parameterValues[i] = firstPos;
                            break;
                        }

                        Binding dataBinding = new Binding("parameterValues[" + i + "]");
                        dataBinding.Mode = BindingMode.TwoWay;
                        dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        dataBinding.Source = parameterValuesObject;

                        valueSelection.SetBinding(ComboBox.SelectedItemProperty, dataBinding);

                        content.Children.Add(selectionPanel);

                    }
                    else
                    {
                        Type uiDisplay = TypeDefinitions.Dictionary[item.ParameterType];
                        var uiObject = (IDataDisplayable)Activator.CreateInstance(uiDisplay);

                        Binding dataBinding = new Binding("parameterValues[" + i + "]");
                        dataBinding.Mode = BindingMode.TwoWay;
                        dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        dataBinding.Source = parameterValuesObject;

                        var parameterType = item.ParameterType;

                        parameterValuesObject.parameterValues[i] = parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null ? Activator.CreateInstance(parameterType) : null;

                        string displayName = item.Name;

                        uiObject.DisplayData(dataBinding, displayName);

                        content.Children.Add((UIElement)uiObject);
                    }

                    i++;
                }

                Button executeButton = new Button();
                executeButton.Content = "Execute";
                executeButton.Margin = Preferences.DefaultMargin;
                executeButton.Width = Preferences.DefaultButtonWidth;

                executeButton.Click += delegate (object sender, System.Windows.RoutedEventArgs e)
                {
                    try
                    {
                        object[] convertFix = new object[parameterValuesObject.parameterValues.Count()];

                        int k = 0;
                        foreach (var item in method.GetParameters())
                        {
                            //The Binding changes the source, as it's defined in an object array. Converting them explicitly helps fixing this problem.
                            convertFix[k] = Convert.ChangeType(parameterValuesObject.parameterValues[k], item.ParameterType);
                            k++;
                        }

                        result = method.Invoke(dataObject, convertFix);
                        ShowResult(result);
                        parameterWindow.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Die Parameter wurden nicht korrent eingegeben.");
                    }
                };

                content.Children.Add(executeButton);
                parameterWindow.Show();
            }
        }

        /// <summary>
        /// Shows the result (the output of a method).
        /// </summary>
        /// <param name="result">The result of the method.</param>
        private static void ShowResult(object result)
        {
            if (result != null)
            {
                Window resultWindow = new Window();
                resultWindow.Background = Preferences.DefaultWindowColor;
                resultWindow.SizeToContent = SizeToContent.WidthAndHeight;
                StackPanel content = new StackPanel();
                content.Orientation = Orientation.Vertical;
                resultWindow.Content = content;

                CreateUIElementForParent(result, content);

                resultWindow.Show();
            }
        }

        public static string GetTitle(object instance, string memberName)
        {
            var realInstance = instance.GetType().GetField("objectInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(instance);
            var titleKey = realInstance.GetType().FullName + "." + memberName;

            if (GeneratorConfiguration.Titles.ContainsKey(titleKey))
            {
                return GeneratorConfiguration.Titles[titleKey];
            }
            else
            {
                return memberName;
            }
        }
    }
}