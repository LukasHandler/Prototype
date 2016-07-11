using System;

namespace Models
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime BirthdayDate { get; set; }

        public string FullName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        public int Age
        {
            get
            {
                int age = DateTime.Now.Year - this.BirthdayDate.Year;

                if (DateTime.Now.Month < this.BirthdayDate.Month || (DateTime.Now.Month == this.BirthdayDate.Month && DateTime.Now.Day < this.BirthdayDate.Day))
                {
                    age--;
                }

                return age;
            }
        }
    }
}
