using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class OrderLine : ICloneable
    {
        public Item Item { get; private set; }
        public uint Quantity { get; set; }

        public double Cost { get { return Quantity * Item.UnitPrice; } }

        private OrderLine() { }

        public OrderLine(Item item, uint quantity)
        {
            this.Item = item;
            this.Quantity = quantity;
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                OrderLine p = (OrderLine)obj;
                return p.Item.Equals(this.Item);
            }
        }

        public object Clone()
        {
            return new OrderLine()
            {
                Item = Item.Clone() as Item,
                Quantity = Quantity
            };
        }
    }
}
