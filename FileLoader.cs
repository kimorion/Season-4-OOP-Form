using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace Program
{
    public class FileLoader
    {

        public List<Customer> LoadCustomersFromFile(string fileName, Action<string> informUser)
        {
            using (var fileStream = File.OpenRead(fileName))
            using (var streamReader = new StreamReader(fileStream))
            {
                StringBuilder builder = new StringBuilder();
                string line;
                int lineNumber = 0;
                List<Customer> result = new List<Customer>();

                while ((line = streamReader.ReadLine()) != null)
                {
                    lineNumber++;

                    var info = line.Split('|');
                    if (info.Length != 4)
                    {
                        builder.AppendLine("Customers file corrupted in the line: " + lineNumber);
                        continue;
                    }

                    var splittedName = info[2].Split(' ');
                    if (splittedName.Length < 3)
                    {
                        builder.AppendLine(
                            "Customers file corrupted in the line (invalid Full Name format): "
                            + lineNumber);
                        continue;
                    }

                    Customer customer = new Customer(
                        info[0],
                        new FullName(splittedName[0], splittedName[1], splittedName[2]),
                        info[1],
                        string.Compare(info[3], "Y", CultureInfo.CurrentCulture, CompareOptions.IgnoreSymbols) == 0 ? Privilege.Premium : Privilege.Common);

                    result.Add(customer);
                }

                if (lineNumber == 0)
                    builder.AppendLine("Customers file was empty");
                if (builder.Length != 0)
                    informUser?.Invoke(builder.ToString());
                return result;
            }
        }

        public List<Item> LoadItemsFromFile(string fileName, Action<string> informUser)
        {
            using (var fileStream = File.OpenRead(fileName))
            using (var streamReader = new StreamReader(fileStream))
            {
                StringBuilder builder = new StringBuilder();
                string line;
                int lineNumber = 0;
                List<Item> result = new List<Item>();

                while ((line = streamReader.ReadLine()) != null)
                {
                    lineNumber++;

                    var info = line.Split('|');
                    if (info.Length != 3)
                    {
                        builder.AppendLine("Items file corrupted in the line: " + lineNumber);
                        continue;
                    }

                    double price;
                    if (!double.TryParse(info[2], out price))
                    {
                        builder.AppendLine("Items file corrupted in the line (wrong price format): " + lineNumber);
                        continue;
                    }

                    result.Add(new Item(info[0], info[1], price));
                }
                if (result.Count == 0)
                    builder.AppendLine("File was empty!");
                if (builder.Length != 0)
                    informUser?.Invoke(builder.ToString());
                return result;
            }
        }
    }
}
