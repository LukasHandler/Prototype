using System.Collections.Generic;

namespace Models
{
    public class Course
    {
        public string CourseName;

        public ICollection<Student> Students;

        public Professor CourseProfessor;

        public Course()
        {
            this.Students = new List<Student>();
        }
    }
}
