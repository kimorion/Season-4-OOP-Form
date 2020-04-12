using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class Item : ICloneable
    {
        public string Article { get; set; }
        public string Name { get; set; }
        public double UnitPrice { get; set; }

        public Item(string article, string name, double unitPrice)
        {
            this.Article = article;
            this.Name = name;
            this.UnitPrice = unitPrice;
        }

        public override int GetHashCode()
        {
            return (Article).GetHashCode();
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
                Item p = (Item)obj;
                return p.Article.Equals(this.Article);
            }
        }

        public object Clone()
        {
            return new Item(Article, Name, UnitPrice);
        }
    }
}
