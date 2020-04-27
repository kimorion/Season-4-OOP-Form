using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Program
{
    public class NodeLabelParser
    {
        public bool TryParseAddress(string label, out string result, Action<string> informUser)
        {
            result = null;
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@' }) != -1)
            {
                informUser?.Invoke("В адресе не должно быть символов ? ! * @");
                return false;
            }

            result = label;
            return true;
        }

        public bool TryParseName(string label, out FullName result, Action<string> informUser)
        {
            result = new FullName();
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@', '.', ',' }) != -1)
            {
                informUser?.Invoke("В ФИО не должно быть символов ? ! * @ . ,");
                return false;
            }

            var splitted = label.Split(' ');
            if (splitted.Length != 3)
            {
                informUser?.Invoke("Введенное ФИО должно состоять из трех слов, разделенных пробелами");
                return false;
            }
            foreach (var word in splitted)
                if (word.Length == 0)
                {
                    informUser?.Invoke("Имя, фамилия и отчество не должны быть пустыми!");
                    return false;
                }

            result = new FullName(splitted);
            return true;
        }

        public bool TryParsePhoneNumber(string label, out string result, Action<string> informUser)
        {
            result = null;
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@', '.', ',' }) != -1)
            {
                informUser?.Invoke("В имени не должно быть символов ? ! * @ . ,");
                return false;
            }

            bool check = label.Any(x => !char.IsDigit(x) && x != '+');
            if (check)
            {
                informUser?.Invoke("Разрешено вводить только цифры и знак '+'");
                return false;
            }

            result = label;
            return true;
        }
    }
}
