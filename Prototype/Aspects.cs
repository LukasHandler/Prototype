using Framework;
using Models;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    [Serializable()]
    public class AgeAspect : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            base.OnSetValue(args);
            Generator.RefreshBindings(args.Instance, "Age");
        }
    }

    [Serializable()]
    public class FullNameAspect : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            base.OnSetValue(args);
            Generator.RefreshBindings(args.Instance, "FullName");
        }
    }

    [Serializable()]
    public class RefreshStudentsAspect : OnMethodBoundaryAspect
    {
        public override void OnExit(MethodExecutionArgs args)
        {
            base.OnExit(args);
            Generator.RefreshBindings(args.Instance, "Students");
            Generator.RefreshBindings(args.Instance, "CanExecuteDeleteStudent");
        }
    }

    [Serializable()]
    public class RefreshStudentsAndCoursesAspect : OnMethodBoundaryAspect
    {
        public override void OnExit(MethodExecutionArgs args)
        {
            base.OnExit(args);

            var instance = (School)args.Instance;

            Generator.RefreshBindings(instance, "Students");
            Generator.RefreshBindings(instance, "CanExecuteDeleteStudent");

            foreach (var course in instance.Courses)
            {
                Generator.RefreshBindings(course, "Students");
            }
        }
    }

    [Serializable()]
    public class RefreshCoursesAspect : OnMethodBoundaryAspect
    {
        public override void OnExit(MethodExecutionArgs args)
        {
            base.OnExit(args);
            Generator.RefreshBindings(args.Instance, "Courses");
        }
    }
}
