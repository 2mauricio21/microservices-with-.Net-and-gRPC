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
            Console.WriteLine("Waiting for server is running");
            Thread.Sleep(2000);

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
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
    }
}
