using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for server is running");
            Thread.Sleep(2000);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var channel = GrpcChannel.ForAddress(_config.GetValue<string>("WorkerService:ServerUrl"));
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                Console.WriteLine("AddProductAsync started...");
                var addProductResponse = await client.AddProductAsync(new AddProductRequest
                {
                    Product = new ProductModel
                    {
                        Name = _config.GetValue<string>("WorkerService:ProductName") + DateTimeOffset.Now,
                        Description = "Apple M1 Pro chip with 10‑core CPU, 16‑core GPU, and 16‑core Neural Engine",
                        Price = 2399.99f,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });

                Console.WriteLine("AddProductAsync Response : " + addProductResponse.ToString());

                //_config.GetValue<int>("WorkerService:TaskInterval"); // pega a configuração do appsettings.json
                await Task.Delay(_config.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }
    }
}
