using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public struct FullName
    {
        public string Name;
        public string Surname;
        public string Patronymic;

        public FullName(string surname, string name, string patronymic)
        {
            this.Surname = surname;
            this.Name = name;
            this.Patronymic = patronymic;
        }

        public FullName(FullName name)
        {
            this.Surname = name.Surname;
            this.Name = name.Name;
            this.Patronymic = name.Patronymic;
        }

        public FullName(string[] name)
        {
            if (name.Length != 3)
                throw new ArgumentException();

            this.Surname = name[0];
            this.Name = name[1];
            this.Patronymic = name[2];
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Surname, Name, Patronymic);
        }
    }
}
