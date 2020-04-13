﻿using System;

namespace Program.Model
{
    public class OrderCostDiscount_1d5K : OrderCostDiscount_1K
    {
        public OrderCostDiscount_1d5K()
        {
            Name = "Скидка по стоимости заказа";
            Description = "Скидка в 10% для заказов от 1500р. ";
        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.1;
        }

        public override Tuple<bool, string> Check(Customer customer, Order order, params string[] exclusiveDiscountFamilies)
        {
            var baseCheck = base.Check(customer, order, exclusiveDiscountFamilies);
            if (!baseCheck.Item1) return baseCheck;

            if (order.TotalCost < 1500)
                return Refusal("Скидка для заказов от 1500р.");
            return Permit();
        }
    }
}