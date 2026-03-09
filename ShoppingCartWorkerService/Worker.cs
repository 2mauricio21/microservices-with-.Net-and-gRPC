using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
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
                // Create SC if not exists
                // Retrieve products from product grpc with server stram
                // Add sc items into SC with client stream

                using var scChanel = GrpcChannel.ForAddress(_config.GetValue<string>("WorkerService:ShoppingCartServerUrl"));
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChanel);

                var scModel = await GetOrCreateShoppingCartAsync(scClient);


                await Task.Delay(_config.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient)
        {
            // try to get sc
            // create sc
            ShoppingCartModel shoppingCartModel;

            try
            {
                _logger.LogInformation("GetShoppinngCartAsync started...");
                shoppingCartModel = await scClient.GetShoppingCartAsync(new GetShoppingCartRequest
                {
                    UserName = _config.GetValue<string>("WorkerService:UserName")
                });

                _logger.LogInformation("GetShoppinngCartAsync Response: {shoppingCartModel}", shoppingCartModel);

            }
            catch (RpcException exception)
            {
                if (exception.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogInformation("CreateShoppingCartAsync started...");
                    shoppingCartModel = await scClient.CreateShoppingCartAsync(new ShoppingCartModel
                    {
                        UserName = _config.GetValue<string>("WorkerService:UserName")
                    });
                    _logger.LogInformation("CreateShoppingCartAsync Response: {shoppingCartModel}", shoppingCartModel);
                }
                else
                {
                    throw exception;
                }
            }

            return shoppingCartModel;
        }
    }
}
