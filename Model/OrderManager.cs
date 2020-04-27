
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Program.Promotion;

namespace Program
{
    public class OrderManager : ICloneable
    {
        private Dictionary<uint, Order> orders = new Dictionary<uint, Order>();

        public int Count()
        {
            return orders.Count;
        }

        public void AddOrder(Order order)
        {
            if (orders.ContainsKey(order.Number))
                throw new Exception("Order with this number already exists");
            orders.Add(order.Number, order);
        }

        public void RemoveOrder(uint number)
        {
            if (!orders.Remove(number))
                throw new Exception("There is no order with the given number");
        }

        public bool TryGetOrder(uint number, out Order order)
        {
            return orders.TryGetValue(number, out order);
        }

        public bool ContainsOrder(uint number)
        {
            return orders.ContainsKey(number);
        }

        public object Clone()
        {
            return new OrderManager
            {
                orders = orders.Clone() as Dictionary<uint, Order>
            };
        }

        public IEnumerable<Order> Orders
        {
            get
            {
                foreach (var item in orders)
                {
                    yield return item.Value;
                }
            }
        }
    }
}
