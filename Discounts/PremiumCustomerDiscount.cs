﻿using System;
using Program.Promotion;

namespace Program.Promotion
{
    public class PremiumCustomerDiscount : Discount
    {
        public PremiumCustomerDiscount() : base("PremiumDiscount")
        {
            Name = "Скидка Premium клиента";
            Description = "Стандартная скидка в 7 % для клиентов со статусов Premium";
        }

        public override double GetDiscountAmount(Order order)
        {
            return order.TotalCost * 0.07;
        }

        public override Tuple<bool, string> Check(Customer customer, Order order)
        {
            if (customer.Privilege < Privilege.Premium)
                return Refusal("Скидка только для клиентов со статусом Premium");
            var baseCheck = base.Check(customer, order);
            if (!baseCheck.Item1) return baseCheck;
            return Permit(baseCheck.Item2);
        }


    }
}
