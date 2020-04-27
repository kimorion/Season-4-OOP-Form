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
            bool result = false;
            if (order.CreationDate.Month == 12 && order.CreationDate.Day >= 25)
                result = true;
            if (order.CreationDate.Month == 1 && order.CreationDate.Day <= 7)
                result = true;

            var baseCheck = base.Check(customer, order);
            if (!baseCheck.Item1) return baseCheck;

            if (result)
                return Permit(baseCheck.Item2);
            else return Refusal("Дата формирования заказа не удовлетворяет сроку действия акции");

        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.12;
        }
    }
}
