using System;

namespace Models
{
    public class Person
    {
        public string FirstName;

        public string LastName;

        public DateTime BirthdayDate;

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
                //http://stackoverflow.com/questions/9/how-do-i-calculate-someones-age-in-c
                var now = DateTime.Now;
                var birthDate = this.BirthdayDate;
                int age = now.Year - birthDate.Year;

                if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                {
                    age--;
                }

                return age;
            }
        }
    }
}
