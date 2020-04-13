using Program.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public enum Privilege { Common, Premium }

    public class Customer : ICloneable
    {
        public string ID { get; set; }
        public FullName Name { get; set; }
        public string ContactPhone { get; set; }
        public Privilege Privilege { get; set; }
        public OrderManager OrderManager { get; private set; } = new OrderManager();

        public List<Discount> discounts = new List<Discount>();

        public Customer(string code, FullName fullName, string contactPhone, Privilege privilege)
        {
            this.Name = fullName;
            this.ContactPhone = contactPhone;
            this.Privilege = privilege;
            this.ID = code;
        }

        private Customer() { }

        public object Clone()
        {
            return new Customer()
            {
                Name = Name,
                ID = ID,
                Privilege = Privilege,
                ContactPhone = ContactPhone,
                OrderManager = OrderManager.Clone() as OrderManager
            };
        }
    }
}
