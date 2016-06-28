using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Framework.Display
{
    public interface IDataDisplayable
    {
        void DisplayData(Binding dataBinding, string displayName);
    }
}
