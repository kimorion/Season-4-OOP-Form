using System;

namespace Program.Promotion
{
    public abstract class Discount : ICloneable
    {
        public readonly string Family;
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string[] exclusiveDiscountFamilies { get; protected set; }

        public Discount(string family) { this.Family = family; }

        public override string ToString()
        {
            return Name;
        }

        public virtual Tuple<bool, string> Check(Customer customer, Order order)
        {
            if (exclusiveDiscountFamilies != null)
                foreach (var family in exclusiveDiscountFamilies)
                {
                    if (order.discounts.TryGetValue(family, out Discount otherDiscount))
                        return Refusal(string.Format("Скидка \"{0}\" несовместима со скидкой \"{1}\"", Name, otherDiscount.Name));
                }

            if (order.discounts.TryGetValue(Family, out Discount relatedDiscount))
            {
                if (GetType().IsSubclassOf(relatedDiscount.GetType()))
                    return Permit(string.Format("Скидка \"{0}\" замещает собой скидку \"{1}\"", Name, relatedDiscount.Name));
                else
                    return Refusal(string.Format("Скидка \"{0}\" замещает собой скидку \"{1}\"", relatedDiscount.Name, Name));
            }
            return Permit("Скидка разрешена");
        }

        public abstract double GetDiscountAmount(Order order);

        protected Tuple<bool, string> Permit(string reason)
        {
            return Tuple.Create(true, reason);
        }

        protected Tuple<bool, string> Permit()
        {
            return Tuple.Create(true, "Скидка разрешена");
        }

        protected Tuple<bool, string> Refusal(string reason)
        {
            return Tuple.Create(false, reason);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
