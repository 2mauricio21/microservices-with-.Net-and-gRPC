using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartDbContext;
        private readonly DiscountService _discountService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(ShoppingCartContext shoppingCartDbContext, DiscountService discountService, IMapper mapper, ILogger<ShoppingCartService> logger)
        {
            _shoppingCartDbContext = shoppingCartDbContext ?? throw new ArgumentNullException(nameof(shoppingCartDbContext));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartDbContext.ShoppingCart
                .FirstOrDefaultAsync(s => s.UserName == request.UserName);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Shopping cart for user {request.UserName} not found."));
            }

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            //var shoppingCartModel = new ShoppingCartModel();
            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            var isExist = await _shoppingCartDbContext.ShoppingCart
                .AnyAsync(s => s.UserName == shoppingCart.UserName);

            if (isExist)
            {
                _logger.LogError("Invalid UserName for ShoppingCart. UserName: {UserName}", shoppingCart.UserName);
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Shopping with Username={request.UserName} is already exists."));
            }

            _shoppingCartDbContext.ShoppingCart.Add(shoppingCart);
            await _shoppingCartDbContext.SaveChangesAsync();

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCart(RemoveItemIntoShoppingCartRequest request, ServerCallContext context)
        {
            // Get sc if exist or not
            // Check item if exist in sc or not
            // Remove item in SC db

            var shoppingCart = await _shoppingCartDbContext.ShoppingCart
                .FirstOrDefaultAsync(s => s.UserName == request.UserName);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Shopping cart for user {request.UserName} not found."));
            }

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == request.RemoveCartItem.ProductId);
            if (removeCartItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId {request.RemoveCartItem.ProductId} not found in shopping cart."));
            }

            shoppingCart.Items.Remove(removeCartItem);
            var removeCount = await _shoppingCartDbContext.SaveChangesAsync();

            var response = new RemoveItemIntoShoppingCartResponse
            {
                Success = removeCount > 0
            };
            return response;
        }

        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, ServerCallContext context)
        {
            // Get sc if exist or not
            // Check the item if exist in sc or not
            //   if item is exist +1 quantity
            //   if item is not exist add new item into sc
            //     Check discont and calculate the item price

            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartDbContext.ShoppingCart
                    .FirstOrDefaultAsync(s => s.UserName == requestStream.Current.UserName);

                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart witch UserName={requestStream.Current.UserName} is not found."));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    // grpc call discount service -- check discount and calculate the item last price
                    var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;

                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartDbContext.SaveChangesAsync();

            var response = new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };

            return response;
        }
    }
}
