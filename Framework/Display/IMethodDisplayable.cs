using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Framework.Display
{
    public interface IMethodDisplayable
    {
        void DisplayMethod(object dataObject, MethodInfo method, Binding canExecuteBinding);
    }
}
