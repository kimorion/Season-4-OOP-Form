using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Program.Promotion;

namespace Program
{
    public delegate void DiscountEventHandler(int orderNumber, string discountName, string reason);

    class DataBase
    {
        public event Action StateChanged;
        public event DiscountEventHandler DiscountDenied;

        private Dictionary<string, Customer> customers;
        private Dictionary<string, Item> items;
        private List<Discount> discounts;

        public bool IsAvailable
        {
            get { return customers != null && items != null; }
        }

        public void Initialize()
        {
            customers = new Dictionary<string, Customer>();
            items = new Dictionary<string, Item>();
            discounts = new List<Discount>();

            discounts.Add(new OrderCostDiscount_1K());
            discounts.Add(new OrderCostDiscount_1d5K());
            discounts.Add(new OrderCostPremiumDiscount_1d5K());
            discounts.Add(new PremiumCustomerDiscount());
            discounts.Add(new WinterDiscount());
        }

        public void Reset()
        {
            customers = null;
            items = null;
        }

        public void CheckCustomerDiscounts(string id)
        {
            if (!customers.TryGetValue(id, out Customer customer))
                throw new KeyNotFoundException();
            foreach (var order in customer.OrderManager.Orders)
            {
                Dictionary<string, Discount> oldDiscounts = order.discounts;
                order.discounts = new Dictionary<string, Discount>();

                foreach (var discount in oldDiscounts.Values)
                {
                    var checkResult = discount.Check(customer, order);
                    if (!checkResult.Item1)
                    {
                        DiscountDenied?.Invoke(order.Number, discount.Name, checkResult.Item2);
                    }
                    else order.discounts.Add(discount.Family, discount);
                }
            }
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

        public Customer GetCustomer(string id)
        {
            customers.TryGetValue(id, out Customer customer);
            return customer?.Clone() as Customer;
        }

        public bool EditCustomer(Customer customer)
        {
            if (!customers.TryGetValue(customer.ID, out Customer result))
            {
                return false;
            }

            customers[customer.ID] = customer.Clone() as Customer;
            CheckCustomerDiscounts(customer.ID);
            StateChanged?.Invoke();
            return true;
        }

        public List<Customer> GetCustomers()
        {
            return customers.Values.ToList().Clone() as List<Customer>;
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


        public void AddItem(Item item)
        {
            if (item == null)
                throw new Exception("Attempt to add null item to the DB");
            if (items == null)
                throw new Exception("Attempt to add item to the unitialized DB");

            items.Add(item.Article, item);
            StateChanged?.Invoke();
        }

        public Item GetItem(string article)
        {
            items.TryGetValue(article, out Item item);
            return item?.Clone() as Item;
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
            CheckCustomerDiscounts(customerID);
            StateChanged?.Invoke();
            return true;
        }


        public List<Discount> GetDiscounts()
        {
            return discounts;
        }

        public Tuple<bool, string> AddDiscount(string customerID, int orderNumber, Discount discount)
        {
            if (!customers.TryGetValue(customerID, out Customer customer))
                throw new Exception("Customer not found in the DB");

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
                throw new Exception("Order with the specified number was not found");

            var checkResult = discount.Check(customer, order);
            if (!checkResult.Item1)
                return checkResult;

            if (order.discounts.TryGetValue(discount.Family, out Discount discount1))
            {
                DiscountDenied?.Invoke(order.Number, discount1.Name, checkResult.Item2);
                order.discounts.Remove(discount.Family);
            }
            order.discounts.Add(discount.Family, discount);
            StateChanged?.Invoke();
            return checkResult;
        }

    }
}
