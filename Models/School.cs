using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class School
    {
        public string Name;

        public ICollection<Student> Students;

        public ICollection<Professor> Professors;

        public ICollection<Course> Courses;

        public School()
        {
            Name = "FH Wiener Neustadt";

            this.Students = new List<Student>();
            var student = new Student() { FirstName = "Lukas", LastName = "Handler", BirthdayDate = new DateTime(1993, 7, 23), MatriculationNumber = 1293129839 };
            this.Students.Add(student);
            student = new Student() { FirstName = "Rene", LastName = "Koch", BirthdayDate = new DateTime(1994, 4, 19), MatriculationNumber = 1293129840 };
            this.Students.Add(student);
            student = new Student() { FirstName = "Kevin", LastName = "Wolf", BirthdayDate = new DateTime(1987, 1, 14), MatriculationNumber = 1293129841 };
            this.Students.Add(student);
            this.Professors = new List<Professor>();
            var prof = new Professor() { FirstName = "Max", LastName = "Mustermann", BirthdayDate = new DateTime(1975, 5, 5), AccountNumber = 123123 };
            this.Professors.Add(prof);

            this.Courses = new List<Course>();
            var course = new Course() { CourseName = "Math", CourseProfessor = prof };
            this.Courses.Add(course);

            foreach (var item in this.Students)
            {
                course.Students.Add(item);
            }
        }

        public void NewStudent(string firstName, string lastName, DateTime birthdayDate, int matriculationNumber)
        {
            this.Students.Add(new Student() { FirstName = firstName, LastName = lastName, BirthdayDate = birthdayDate, MatriculationNumber = matriculationNumber });
        }

        public void DeleteStudent(Student toDelete)
        {
            this.Students.Remove(toDelete);
            this.Courses.Where(p => p.Students.Contains(toDelete)).ToList().ForEach(p => p.Students.Remove(toDelete));
        }

        public void NewCourse(string courseName, Professor courseProfessor)
        {
            var course = new Course() { CourseName = courseName, CourseProfessor = courseProfessor };
            this.Courses.Add(course);
        }

        public void AddStudentToCourse(Student student, Course course)
        {
            if (!course.Students.Contains(student))
            {
                course.Students.Add(student);
            }
        }
    }
}
