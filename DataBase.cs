using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Program
{

    class DataBase
    {
        public event Action StateChanged;

        private Dictionary<string, Customer> customers;
        private Dictionary<string, Item> items;

        public bool IsAvailable
        {
            get { return customers != null && items != null; }
        }

        public void Initialize()
        {
            customers = new Dictionary<string, Customer>();
            items = new Dictionary<string, Item>();
        }

        public void Reset()
        {
            customers = null;
            items = null;
        }

        public void AddCustomer(Customer customer)
        {
            if (customer == null)
                throw new Exception("Attempt to add null customer to the DB");
            if (customers == null)
                throw new Exception("Attempt to add customer to the unitialized DB");
            customers.Add(customer.ID, customer);
            StateChanged?.Invoke();
        }

        public void AddItem(Item item)
        {
            if (item == null)
                throw new Exception("Attempt to add null item to the DB");
            if (items == null)
                throw new Exception("Attempt to add item to the unitialized DB");

            items.Add(item.Article, item);
            StateChanged?.Invoke();
        }

        public Customer GetCustomer(string code)
        {
            customers.TryGetValue(code, out Customer customer);
            return customer?.Clone() as Customer;
        }

        public Item GetItem(string article)
        {
            items.TryGetValue(article, out Item item);
            return item?.Clone() as Item;
        }

        /// <summary>
        /// Searches a db with a code of the given customer,  
        /// and if it finds it, it will replace it with a clone of the given customer.
        /// </summary>
        /// <param name="customer">Edited customer</param>
        /// <returns>Operation status</returns>
        public bool EditCustomer(Customer customer)
        {
            if (!customers.TryGetValue(customer.ID, out Customer result))
            {
                return false;
            }

            customers[customer.ID] = customer.Clone() as Customer;
            StateChanged?.Invoke();
            return true;
        }

        public bool EditItem(Item item)
        {
            if (!items.TryGetValue(item.Article, out Item result))
            {
                return false;
            }

            items[item.Article] = item.Clone() as Item;
            StateChanged?.Invoke();
            return true;
        }

        public bool EditOrder(string customerID, Order order)
        {
            if (!customers.TryGetValue(customerID, out Customer customer))
            {
                return false;
            }
            if (!customer.OrderManager.TryGetOrder(order.Number, out Order originalOrder))
            {
                return false;
            }
            customer.OrderManager.EditOrder(order.Clone() as Order);
            StateChanged?.Invoke();
            return true;
        }

        public IEnumerable<Customer> Customers
        {
            get
            {
                foreach (var customer in customers)
                {
                    yield return customer.Value.Clone() as Customer;
                }
            }
        }

        public IEnumerable<Item> Items
        {
            get
            {
                foreach (var item in items)
                {
                    yield return item.Value.Clone() as Item;
                }
            }
        }

        public List<Customer> GetCustomers()
        {
            return customers.Values.ToList().Clone() as List<Customer>;
        }

        public List<Item> GetItems()
        {
            return items.Values.ToList().Clone() as List<Item>;
        }

        public bool DeleteItem(Item item)
        {
            if (items.Remove(item.Article))
            {
                StateChanged?.Invoke();
                return true;
            }
            return false;
        }
    }
}
