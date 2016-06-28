using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
	[Serializable()]
    public class UpdateBindingAspect : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            base.OnSetValue(args);
            Generator.RefreshBindings(args.Instance, args.LocationName);
        }
    }
}
