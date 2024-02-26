using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    internal class Employee
    {
        // JSON-objekt, använd egenskaper istället för instanser?
        public int Salary { get;  set; }
        public string firstName { get;  set; }

        public Employee(int salary, string firstname)
        {
            Salary = salary;
            firstName = firstname;
        }
    }
}
