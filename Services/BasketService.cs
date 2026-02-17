using Chinese_sale_api.Data;
using Chinese_sale_api.DTO;
using Chinese_sale_api.DTOs;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.OpenApi.Extensions;
using Chinese_sale_api.Exceptions;

using Chinese_sale_api.Repositories; 
using System;
using System.Data;
using System.Security.Claims;
using projectApiAngular.Repositories;

namespace Chinese_sale_api.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BasketService> _logger;
        private readonly IPurchasesService _purchasesService;
        private readonly IGiftRepository _giftRepository;
        private const string ClassName = nameof(BasketService);

        public BasketService(IBasketRepository basketRepository, IPurchasesService purchasesService, IHttpContextAccessor httpContextAccessor, ILogger<BasketService> logger, IGiftRepository giftRepository)
        {
            _basketRepository = basketRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _purchasesService = purchasesService;
            _giftRepository = giftRepository;
        }

        //map to dto
        private static ReadBasketDto Map(Basket b)
        {
            return new ReadBasketDto
            {
                Id = b.Id,
                amount = b.amount,
                UserId = b.UserId,
                user = new ReadUserDto
                {
                    Id = b.UserId,
                    Name = b.User.Name,
                    Email = b.User.Email,
                    Phone = b.User.Phone,
                    Role = CustomerRole.User.GetDisplayName()

                },
                GiftId = b.GiftId,
                gift = new ReadGiftDTO
                {
                    Name = b.gift.Name,
                    Description = b.gift.Description,
                    Price = b.gift.Price,
                    ImagePath = b.gift.ImagePath,
                    CategoryName = b.gift.Category.Name,
                    DonorName = b.gift.Donor.Name,
                    DonorId = b.gift.DonorId,
                    CategoryId = b.gift.CategoryId
                },
            };
        }

        //get current user id
        private int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException();

            var userIdClaim = user.FindFirst("id");
            if (userIdClaim == null)
                throw new Exception("User Id claim missing");

            return int.Parse(userIdClaim.Value);
        }

        //get the current user's basket items   
        public async Task<IEnumerable<ReadBasketDto>> GetMyBasket()
        {
            int userId = GetCurrentUserId();
            LoggingHelper.LogMethodStart(_logger, nameof(GetMyBasket), ClassName, new { UserId = userId });

            var baskets = await _basketRepository.GetMyBasketAsync(userId);
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetMyBasket), ClassName, baskets.Count());

            return baskets.Select(Map);
        }

        //Check if gift has a winner
        private async Task<User?> GetGiftWinnerAsync(int giftId)
        {
            var gift = await _giftRepository.GetGiftByIdAsync(giftId);
            if (gift == null)
                return null;

            if (gift.WinnerId != null)
                return gift.Winner;

            return null;
        }

        //EnterToBasketAsync
        public async Task<ReadBasketDto?> EnterToBasketAsync(CreateBasketDto basketDto)
        {
            int userId = GetCurrentUserId();
            LoggingHelper.LogMethodStart(_logger, nameof(EnterToBasketAsync), ClassName, new { UserId = userId, GiftId = basketDto.GiftId, Amount = basketDto.amount });

            try
            {
                var winner = await GetGiftWinnerAsync(basketDto.GiftId);
                if (winner != null)
                {
                    LoggingHelper.LogDuplicateAttempt(_logger, nameof(EnterToBasketAsync), ClassName, $"GiftId: {basketDto.GiftId}");
                    throw new GiftAlreadyAsignedException(winner.Name);
                }

                var entity = new Basket
                {
                    UserId = userId,
                    GiftId = basketDto.GiftId,
                    amount = basketDto.amount,
                };
                var basket = await _basketRepository.EnterToBasketAsync(entity);
                LoggingHelper.LogCreated(_logger, nameof(EnterToBasketAsync), ClassName, new { basket.Id, UserId = userId, basket.GiftId });
                return new ReadBasketDto { Id = basket.Id, amount = basket.amount, GiftId = basket.GiftId, UserId = userId };
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(EnterToBasketAsync), ClassName, ex);
                throw new Exception(ex.Message);
            }
        }

        //update amount
        public async Task<ReadBasketDto?> UpdateBasketAmountAsync(int id, int newAmount)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(UpdateBasketAmountAsync), ClassName, new { BasketId = id, NewAmount = newAmount });

            if (newAmount < 0 || newAmount > 1000)
            {
                LoggingHelper.LogValidationError(_logger, nameof(UpdateBasketAmountAsync), ClassName, $"Invalid amount {newAmount}. Must be between 0 and 1000.");
                throw new ArgumentOutOfRangeException();
            }
            var basket = await _basketRepository.UpdateBasketAmountAsync(id, newAmount);
            if (basket == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(UpdateBasketAmountAsync), ClassName, id.ToString());
                return null;
            }

            LoggingHelper.LogUpdated(_logger, nameof(UpdateBasketAmountAsync), ClassName, new { BasketId = id, NewAmount = newAmount });
            return Map(basket);
        }

        //delete basket
        public async Task<int?> DeleteBasketAsync(int id)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(DeleteBasketAsync), ClassName, new { BasketId = id });
            var basket = await _basketRepository.DeleteBasketAsync(id);

            if (basket == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(DeleteBasketAsync), ClassName, id.ToString());
                return null;
            }

            LoggingHelper.LogDeleted(_logger, nameof(DeleteBasketAsync), ClassName, id.ToString());
            return basket.Id;
        }

        public async Task<bool> BuyAllBasketsAsync()
        {
            int userId = GetCurrentUserId();
            LoggingHelper.LogMethodStart(_logger, nameof(BuyAllBasketsAsync), ClassName, new { UserId = userId });

            var baskets = await _basketRepository.GetMyBasketAsync(userId);
            LoggingHelper.LogMethodWithCount(_logger, $"{nameof(BuyAllBasketsAsync)}_FetchBaskets", ClassName, baskets.Count());

            if (!baskets.Any())
            {
                LoggingHelper.LogValidationError(_logger, nameof(BuyAllBasketsAsync), ClassName, $"No basket items for user {userId}");
                return false;
            }

            // Check if any gift in the basket has a winner
            foreach (var basket in baskets)
            {
                var winner = await GetGiftWinnerAsync(basket.GiftId);
                if (winner != null)
                {
                    LoggingHelper.LogDuplicateAttempt(_logger, nameof(BuyAllBasketsAsync), ClassName, $"GiftId: {basket.GiftId}");
                    throw new GiftAlreadyAsignedException(winner.Name);
                }
            }

            //transaction
            using var transaction = await _basketRepository.beginTransactionAsync();
            try
            {
                foreach (var basket in baskets)
                {
                    for (int i = 0; i < basket.amount; i++)
                    {
                        var p = await _purchasesService.AddPurchaseAsync(new CreatePurchaseDto { CustomerId = userId, GiftId = basket.GiftId, PurchDate = DateTime.Now });
                        if (p == null)
                        {
                            throw new Exception($"failed to purchase gift {basket.GiftId} in basket {basket.Id}");
                        }
                    }
                    var deleted = await DeleteBasketAsync(basket.Id);
                    if(deleted == null)
                    {
                        throw new Exception($"failed to delete basket {basket.Id}");
                    }
                }
                await transaction.CommitAsync();
                LoggingHelper.LogMethodSuccess(_logger, nameof(BuyAllBasketsAsync), ClassName, new { UserId = userId, PurchaseCount = baskets.Sum(b => b.amount) });
                return true;

            } 
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                LoggingHelper.LogUnexpectedError(_logger, nameof(BuyAllBasketsAsync), ClassName, ex);
                return false;
            }
        }
    }
}

