using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductsContext _productsContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductsContext productsContext, IMapper mapper, ILogger<ProductService> logger)
        {
            _productsContext = productsContext ?? throw new ArgumentNullException(nameof(productsContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productsContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found."));
            }

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productsContext.Product.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = _mapper.Map<ProductModel>(product);

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            //var product = new Product
            //{
            //    ProductId = request.Product.ProductId,
            //    Name = request.Product.Name,
            //    Description = request.Product.Description,
            //    Price = request.Product.Price,
            //    Status = Models.ProductStatus.INSTOCK,
            //    CreatedTime = request.Product.CreatedTime.ToDateTime()
            //};

            var product = _mapper.Map<Product>(request.Product);

            _productsContext.Product.Add(product);
            await _productsContext.SaveChangesAsync();

            //var productModel = new ProductModel
            //{
            //    ProductId = product.ProductId,
            //    Name = product.Name,
            //    Description = product.Description,
            //    Price = product.Price,
            //    Status = Protos.ProductStatus.Instock,
            //    CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            //};
            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }
    }
}
