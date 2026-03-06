using ProductGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductGrpc.Data
{
    public class ProductsContextSeed
    {
        public static void SeedAsync(ProductsContext productsContext)
        {
            if (!productsContext.Product.Any())
            {
                var products = new List<Product> {
                    new Product
                    {
                        ProductId = 1,
                        Name = "iPhone 12",
                        Description = "Apple iPhone 12 with 5G speed.",
                        Price = 799.99f,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        ProductId = 2,
                        Name = "Samsung Galaxy S21",
                        Description = "Samsung Galaxy S21 with 5G speed.",
                        Price = 699.99f,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        ProductId = 3,
                        Name = "Google Pixel 5",
                        Description = "Google Pixel 5 with 5G speed.",
                        Price = 599.99f,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    }
                };
                productsContext.Product.AddRange(products);
                productsContext.SaveChanges();
            }
        }
    }
}
