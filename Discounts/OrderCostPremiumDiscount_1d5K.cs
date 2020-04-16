using System;

namespace Program.Promotion
{
    public class OrderCostPremiumDiscount_1d5K : OrderCostDiscount_1d5K
    {
        public OrderCostPremiumDiscount_1d5K()
        {
            Name = "Premium скидка от 1500р.";
            Description = "Скидка в 15% для заказов от 1500р. для клиентов со статусом Premium";
        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.15;
        }

        public override Tuple<bool, string> Check(Customer customer, Order order)
        {
            if (customer.Privilege < Privilege.Premium)
                return Refusal("Скидка только для клиентов со статусом Premium");
            if (order.TotalCost < 1500)
                return Refusal("Скидка для заказов стоимостью от 1500р.");
            var baseCheck = base.Check(customer, order);
            if (!baseCheck.Item1) return baseCheck;
            return Permit();
        }
    }
}
