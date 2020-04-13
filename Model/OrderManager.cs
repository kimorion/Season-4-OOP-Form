using Program.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class OrderManager : ICloneable
    {
        private Dictionary<int, Order> orders = new Dictionary<int, Order>();

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

        public void Remove(int number)
        {
            if (!orders.Remove(number))
                throw new Exception("There is no order with the given number");
        }

        public bool TryGetOrder(int number, out Order order)
        {
            return orders.TryGetValue(number, out order);
        }

        public bool ContainsOrder(int number)
        {
            return orders.ContainsKey(number);
        }

        public bool EditOrder(Order order)
        {
            if (!orders.ContainsKey(order.Number))
            {
                throw new Exception("There is no order with the given number");
            }
            orders[order.Number] = order;
            return true;
        }

        public object Clone()
        {
            return new OrderManager
            {
                orders = orders.Clone() as Dictionary<int, Order>
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

        public double GetOrderCost(int number)
        {
            throw new NotImplementedException();
        }

        public double GetOrderCost(int number, List<Discount> discounts)
        {
            throw new NotImplementedException();
        }
    }
}
