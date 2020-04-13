using System;
using Program.Promotion;

namespace Program.Promotion
{
    public class WinterDiscount : Discount
    {
        public WinterDiscount() : base("WinterDiscount")
        {
            Name = "Новогодняя скидка";
            Description = "Скидка для заказов, сформированных в период с 25 декабря по 7 января";
        }

        public override Tuple<bool, string> Check(Customer customer, Order order)
        {
            var baseCheck = base.Check(customer, order);
            if (!baseCheck.Item1) return baseCheck;

            if (order.CreationDate.Month == 12)
                if (order.CreationDate.Day >= 25)
                    return Permit();
            if (order.CreationDate.Month == 1)
                if (order.CreationDate.Day <= 7)
                    return Permit();
            return Refusal("Дата формирования заказа не удовлетворяет сроку действия акции");

        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.12;
        }
    }
}
