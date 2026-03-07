using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await GetAllProductsAsync(client);
            await AddProductAsync(client);

            await UpdateProductAsync(client);
            await DeleteProductAsync(client);

            await GetAllProductsAsync(client);
            await InsertBulkProduct(client);
            await GetAllProductsAsync(client);

            Console.ReadKey();
        }

        private static async Task GetAllProductsAsync(ProductProtoService.ProductProtoServiceClient client)
        {

            // GetAllProductsAsync
            //Console.WriteLine("GetAllProductsAsync started..");
            //using (var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            //{
            //    while (await clientData.ResponseStream.MoveNext(new CancellationToken()))
            //    {
            //        var currentProduct = clientData.ResponseStream.Current;
            //        Console.WriteLine(currentProduct);
            //    }
            //}

            // GetAllProducts witch C# 9
            Console.WriteLine("GetAllProductsAsync witch C# 9 started..");
            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {

            // GetProductAsync
            Console.WriteLine("GetProductAsync started..");
            var responde = await client.GetProductAsync(
                new GetProductRequest
                {
                    ProductId = 1
                });

            Console.WriteLine("GetProductAsync Response : " + responde.ToString());
        }


        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            // AddProductAsync
            Console.WriteLine("AddProductAsync started...");
            var addProductResponse = await client.AddProductAsync(new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = "Apple MacBook Pro 16 inch",
                    Description = "Apple M1 Pro chip with 10‑core CPU, 16‑core GPU, and 16‑core Neural Engine",
                    Price = 2399.99f,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine("AddProductAsync Response : " + addProductResponse.ToString());
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            // UpdateProductAsync
            Console.WriteLine("UpdateProductAsync started...");
            var updateProductResponse = await client.UpdateProductAsync(new UpdateProductRequest
            {
                Product = new ProductModel
                {
                    ProductId = 1,
                    Name = "Apple MacBook Pro 16 inch",
                    Description = "Apple M1 Pro chip with 10‑core CPU, 16‑core GPU, and 16‑core Neural Engine",
                    Price = 2399.99f,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine("UpdateProductAsync Response : " + updateProductResponse.ToString());
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            // DeleteProductAsync
            Console.WriteLine("DeleteProductAsync started...");
            var deleteProductResponse = await client.DeleteProductAsync(
                new DeleteProductRequest
                {
                    ProductId = 3
                });

            Console.WriteLine("DeleteProductAsync Response : " + deleteProductResponse.Success.ToString());
            Thread.Sleep(1000);
        }


        private static async Task InsertBulkProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            // InsertBulkProduct
            Console.WriteLine("InsertBulkProduct started...");
            using var clientBulk = client.InsertBulkProduct();
            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"Product {i + 1}",
                    Description = $"Bulk inserted product {i + 1}",
                    Price = (i + 1) * 100,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await clientBulk.RequestStream.WriteAsync(productModel);

            }

            await clientBulk.RequestStream.CompleteAsync();
            var responseBulk = await clientBulk;
            Console.WriteLine($"Status: {responseBulk.Success}. Inserted Count: {responseBulk.InsertedCount}");
        }


    }
}
