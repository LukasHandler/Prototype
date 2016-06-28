using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Framework.Display.DefaultTypes;
using System.Collections;
using Framework.Display;

namespace Framework
{
    public static class TypeDefinitions
    {
        /// <summary>
        /// Holds the type of an user control to represent methods. Must implement the "IMethodDisplayAble"-interface.
        /// </summary>
        internal static Type MethodDisplay;

        /// <summary>
        /// The dictionary to match a ui element to a dataType. The ui elements need to inherit from "UIElement" and implement the "IDataDisplayAble"-interface.
        /// </summary>
        internal static IDictionary<Type, Type> Dictionary;

        static TypeDefinitions()
        {
            Dictionary = new Dictionary<Type, Type>();

            RegisterElement(typeof(string), typeof(DefaultTextDisplay));
            RegisterElement(typeof(int), typeof(DefaultTextDisplay));
            RegisterElement(typeof(DateTime), typeof(DefaultDateTimeDisplay));
            RegisterElement(typeof(IEnumerable), typeof(DefaultIEnumerableDisplay));

            RegisterMethodElement(typeof(DefaultMethodDisplay));
        }

        /// <summary>
        /// Registers a ui element for a specific dataType. Overwrites existing definitions for dataTypes.
        /// </summary>
        /// <param name="dataType">The type the ui element should be used for.</param>
        /// <param name="elementType">The type of the ui element.</param>
        public static void RegisterElement(Type dataType, Type elementType)
        {
            if (elementType.IsSubclassOf(typeof(UIElement)) && elementType.GetInterfaces().Any(p => p == typeof(IDataDisplayable)))
            {
                if (Dictionary.ContainsKey(dataType))
                {
                    Dictionary.Remove(dataType);
                }

                Dictionary.Add(dataType, elementType);
            }
        }

        /// <summary>
        /// Registers the method ui element.
        /// </summary>
        /// <param name="elementType">Type of the ui element to represent a method.</param>
        public static void RegisterMethodElement(Type elementType)
        {
            if (elementType.IsSubclassOf(typeof(UIElement)) && elementType.GetInterfaces().Any(p => p == typeof(IMethodDisplayable)))
            {
                MethodDisplay = elementType;
            }
        }
    }
}
