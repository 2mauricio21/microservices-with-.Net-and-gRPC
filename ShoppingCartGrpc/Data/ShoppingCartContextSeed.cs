using ShoppingCartGrpc.Models;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static void SeedAsync(ShoppingCartContext shoppingCartContext)
        {
            if (!shoppingCartContext.ShoppingCart.Any())
            {
                var shoppingCarts = new List<ShoppingCart>
                {
                    new ShoppingCart
                    {
                        UserName = "swn",
                        Items = new List<ShoppingCartItem>
                        {
                            new ShoppingCartItem
                            {
                                Quantity = 2,
                                Color = "Black",
                                Price = 10.0f,
                                ProductId = 1,
                                ProductName = "Product 1",
                            },
                            new ShoppingCartItem
                            {
                                Quantity = 3,
                                Color = "Red",
                                Price = 20.0f,
                                ProductId = 2,
                                ProductName = "Product 2",
                            }
                        }
                    }
                };
                shoppingCartContext.ShoppingCart.AddRange(shoppingCarts);
                shoppingCartContext.SaveChanges();
            }

        }
    }
}
