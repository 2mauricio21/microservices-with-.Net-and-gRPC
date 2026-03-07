using DiscountGrpc.Model;
using System.Collections.Generic;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discounts = new List<Discount>
        {
            new Discount { DiscountId = 1, Code = "Code_100", Amount = 100 },
            new Discount { DiscountId = 2, Code = "Code_200", Amount = 200 },
            new Discount { DiscountId = 3, Code = "Code_300", Amount = 300 }
        };
    }
}
