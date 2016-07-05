using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Framework
{
    public class Configuration
    {
        private Dictionary<Type, string[]> dataTemplates;

        private Dictionary<Tuple<string, string>, object> parameterCollections;

        private Dictionary<string, Func<bool>> canExecuteCollection;

        private List<string> hidingMembers;

        private Dictionary<string, string> titles;

        private Dictionary<string, List<Tuple<string, object>>> uiPropertyOverwriteValues;

        private Dictionary<string, Configuration> memberConfigurators;

        private Dictionary<string, Type> uiElements;

        private Dictionary<Type, Func<object, Type, object>> wrapperConverter;
 
        public Configuration()
        {
            this.dataTemplates = new Dictionary<Type, string[]>();
            this.parameterCollections = new Dictionary<Tuple<string, string>, object>();
            this.canExecuteCollection = new Dictionary<string, Func<bool>>();
            this.hidingMembers = new List<string>();
            this.titles = new Dictionary<string, string>();
            this.uiPropertyOverwriteValues = new Dictionary<string, List<Tuple<string, object>>>();
            this.memberConfigurators = new Dictionary<string, Configuration>();
            this.uiElements = new Dictionary<string, Type>();
            this.wrapperConverter = new Dictionary<Type, Func<object, Type, object>>();

            Func<object, Type, object> convertToWrapper = delegate (object source, Type resultWrapper)
            {
                var genericInstance = Activator.CreateInstance(resultWrapper);

                foreach (var item in (ICollection)source)
                {
                    var wrapperItem = Generator.GetWrapper(item);
                    genericInstance.GetType().GetMethod("Add").Invoke(genericInstance, new[] { wrapperItem });
                }

                return genericInstance;
            };

            this.AddConverter(typeof(ICollection), convertToWrapper);
        }

        public Dictionary<Type, string[]> DataTemplates
        {
            get
            {
                return this.dataTemplates;
            }
        }

        public Dictionary<Tuple<string, string>, object> ParameterCollections
        {
            get
            {
                return this.parameterCollections;
            }
        }

        public Dictionary<string, Func<bool>> CanExecuteCollection
        {
            get
            {
                return this.canExecuteCollection;
            }
        }

        public Dictionary<string, string> Titles
        {
            get
            {
                return this.titles;
            }
        }

        public List<string> HidingMembers
        {
            get
            {
                return this.hidingMembers;
            }
        }

        public Dictionary<string, List<Tuple<string, object>>> UIPropertyOverwriteValues
        {
            get
            {
                return this.uiPropertyOverwriteValues;
            }
        }

        public Dictionary<string, Configuration> MemberConfigurators
        {
            get
            {
                return this.memberConfigurators;
            }
        }

        public Dictionary<string, Type> UIElements
        {
            get
            {
                return this.uiElements;
            }
        }

        public Dictionary<Type, Func<object, Type, object>> WrapperConverter
        {
            get
            {
                return this.wrapperConverter;
            }
        }

        public void AddTemplate(Type dataType, params string[] displayValues)
        {
            dataTemplates[dataType] = displayValues;
        }

        public void AddParameterCollection(string methodName, string parameterName, IEnumerable collection)
        {
            this.parameterCollections[new Tuple<string, string>(methodName, parameterName)] = collection;
        }

        public void AddCanExecuteFunction(Func<bool> canExecuteFunction, params string[] methodNames)
        {
            foreach (var methodName in methodNames)
            {
                this.canExecuteCollection[methodName] = canExecuteFunction;
            }
        }

        public void HideMember(string memberName)
        {
            this.HidingMembers.Add(memberName);
        }

        public void AddTitle(string memberName, string title)
        {
            this.titles[memberName] = title;
        }

        public void UIPropertyOvewrite(string memberName, string uiProperty, object value)
        {
            if (!this.uiPropertyOverwriteValues.ContainsKey(memberName))
            {
                this.uiPropertyOverwriteValues[memberName] = new List<Tuple<string, object>>();
            }

            this.uiPropertyOverwriteValues[memberName].Add(new Tuple<string, object>(uiProperty, value));
        }

        public void AddMemberConfiguration(string memberName, Configuration configurator)
        {
            this.memberConfigurators[memberName] = configurator;
        }

        public void UseUIElement(string memberName, Type uiElement)
        {
            this.uiElements[memberName] = uiElement;
        }

        public void AddConverter(Type sourceType, Func<object, Type, object> convertToWrapper)
        {
            this.wrapperConverter[sourceType] = convertToWrapper;
        }
    }
}
