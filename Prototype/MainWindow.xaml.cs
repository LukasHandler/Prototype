using Framework;
using Models;
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
using PostSharp.Patterns.Model;
using System.ComponentModel;
using Framework.Display.DefaultTypes;

namespace Example
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private School exampleSchool;

        public School ExampleSchool
        {
            get
            {
                if (this.exampleSchool == null)
                {
                    this.exampleSchool = new School();
                }

                return this.exampleSchool;
            }
        }

        public MainWindow()
        {
            this.ConfigurateGenerator();
            this.InitializeComponent();
        }

        public void ConfigurateGenerator()
        {
            Preferences.DefaultWindowColor = Brushes.LightBlue;

            Configuration config = new Configuration();
            config.AddTemplate(typeof(Student), "FirstName", "LastName");
            config.AddTemplate(typeof(Professor), "FirstName", "LastName");
            config.AddTemplate(typeof(Course), "CourseName");

            config.AddParameterCollection("Models.School.DeleteStudent", "toDelete", this.ExampleSchool.Students);
            config.AddParameterCollection("Models.School.AddStudentToCourse", "student", this.ExampleSchool.Students);
            config.AddParameterCollection("Models.School.AddStudentToCourse", "course", this.ExampleSchool.Courses);
            config.AddParameterCollection("Models.School.NewCourse", "courseProfessor", this.ExampleSchool.Professors);

            Func<bool> canDeleteStudent = delegate ()
            {
                return this.ExampleSchool.Students.Count != 0;
            };

            config.AddCanExecuteFunction(canDeleteStudent, "Models.School.DeleteStudent", "Models.School.AddStudentToCourse");

            config.UIPropertyOvewrite("Models.School.Name", "Background", Brushes.AliceBlue);

            Configuration directorConfigurator = new Configuration();
            directorConfigurator.HideMember("Models.Professor.AccountNumber");
            config.AddMemberConfiguration("Models.Course.CourseProfessor", directorConfigurator);

            Generator.GeneratorConfiguration = config;
        }
    }
}
