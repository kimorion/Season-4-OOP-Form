using System;
using Program.Promotion;

namespace Program.Promotion
{
    public class OrderCostDiscount_1K : Discount
    {
        public OrderCostDiscount_1K() : base("CostBasedDiscount")
        {
            Name = "Скидка для заказов от 1000р.";
            Description = "Скидка в 5% для заказов от 1000р. ";
        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.05;
        }

        public override Tuple<bool, string> Check(Customer customer, Order order)
        {
            var baseCheck = base.Check(customer, order);
            if (!baseCheck.Item1) return baseCheck;

            if (order.TotalCost < 1000)
                return Refusal("Заказ дешевле 1000 рублей");
            return Permit();

        }


    }
}
