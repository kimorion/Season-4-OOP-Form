﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Program.Promotion;

namespace Program
{
    public enum DeliveryType { Standard, Express }

    public class Order : ICloneable
    {
        private HashSet<OrderLine> orderLines = new HashSet<OrderLine>();
        public Dictionary<string, Discount> discounts = new Dictionary<string, Discount>();

        public string Address { get; set; }
        public DateTimeOffset CreationDate { get; set; } = new DateTimeOffset();
        public DeliveryType DeliveryType { get; set; }
        public int Number { get; private set; }
        public IEnumerable<OrderLine> OrderLines
        {
            get
            {
                foreach (var line in orderLines)
                {
                    yield return line;
                }
            }
        }

        public double TotalCost
        {
            get
            {
                double result = 0;

                foreach (var line in orderLines)
                {
                    result += line.Cost;
                }

                if (DeliveryType == DeliveryType.Express) result *= 1.25;
                return result;
            }
        }

        public double TotalDiscount
        {
            get
            {
                double discountAmount = 0;
                foreach (var discount in discounts.Values)
                {
                    discountAmount += discount.GetDiscountAmount(this);
                }
                return discountAmount;
            }
        }

        public int OrderLinesAmount
        {
            get { return orderLines.Count(); }
        }

        public int ItemsAmount
        {
            get
            {
                int result = 0;
                foreach (var line in orderLines)
                {
                    result += (int)line.Quantity;
                }
                return result;
            }
        }

        private Order() { }

        public Order(int number, string address, DeliveryType type)
        {
            this.Number = number;
            this.CreationDate = DateTimeOffset.Now;
            this.Address = address;
            this.DeliveryType = type;
        }

        public void AddOrderLine(OrderLine line)
        {
            OrderLine current;
            if (orderLines.TryGetValue(line, out current))
            {
                current.Quantity += line.Quantity;
            }
            else orderLines.Add(line);
        }

        public void AddItem(Item item, uint quantity)
        {
            OrderLine line = new OrderLine(item, quantity);
            AddOrderLine(line);
        }

        public bool DeleteItem(Item item)
        {
            OrderLine line = new OrderLine(item, 0);
            return orderLines.Remove(line);
        }

        public void SetItemQuantity(Item item, uint quantity)
        {
            OrderLine current;
            if (orderLines.TryGetValue(new OrderLine(item, 0), out current))
            {
                current.Quantity = quantity;
            }
            else throw new Exception("The specified item was not found");
        }

        public object Clone()
        {
            return new Order()
            {
                Address = Address,
                Number = Number,
                DeliveryType = DeliveryType,
                CreationDate = CreationDate,
                orderLines = orderLines.Clone() as HashSet<OrderLine>,
                discounts = discounts.Clone() as Dictionary<string, Discount>
            };
        }
    }
}
