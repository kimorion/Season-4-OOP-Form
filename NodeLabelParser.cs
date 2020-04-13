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
        public bool TryParseAddress(string label, out string result)
        {
            result = null;
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@' }) != -1)
                return false;

            result = label;
            return true;
        }

        public bool TryParseName(string label, out FullName result)
        {
            result = new FullName();
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@', '.', ',' }) != -1)
                return false;

            var splitted = label.Split(' ');
            if (splitted.Length != 3) return false;
            foreach (var word in splitted)
                if (word.Length == 0) return false;

            result = new FullName(splitted);
            return true;
        }

        public bool TryParsePhoneNumber(string label, out string result)
        {
            result = null;
            if (label.IndexOfAny(new char[] { '?', '!', '*', '@', '.', ',' }) != -1)
                return false;

            bool check = label.Any(x => !char.IsDigit(x) && x != '+');
            if (check)
                return false;

            result = label;
            return true;
        }


    }
}
