using System.Collections.Generic;

namespace Models
{
    public class Course
    {
        public string CourseName { get; set; }

        public ICollection<Student> Students { get; set; }

        public Professor CourseProfessor { get; set; }

        public Course()
        {
            this.Students = new List<Student>();
        }
    }
}
