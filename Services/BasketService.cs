using Chinese_sale_api.Data;
using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Microsoft.OpenApi.Extensions;
using projectApiAngular.Repositories;
using System;
using System.Data;
using System.Security.Claims;

namespace Chinese_sale_api.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BasketService> _logger;
        public BasketService(IBasketRepository basketRepository, IHttpContextAccessor httpContextAccessor, ILogger<BasketService> logger)
        {
            _basketRepository = basketRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
            _logger.LogInformation("Fetching basket for user {UserId}.", userId);

            var baskets = await _basketRepository.GetMyBasketAsync(userId);
            _logger.LogInformation("Retrieved {Count} basket items for user {UserId}.", baskets.Count(), userId);
            
            return baskets.Select(Map);
        }

        //EnterToBasketAsync
        public async Task<ReadBasketDto?> EnterToBasketAsync(CreateBasketDto basketDto)
        {
            int userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} is adding gift {GiftId} amount {Amount} to basket.", userId, basketDto.GiftId, basketDto.amount);

            try
            {
                var entity = new Basket
                {

                    UserId = userId,
                    GiftId = basketDto.GiftId,
                    amount = basketDto.amount,

                };

                var basket = await _basketRepository.EnterToBasketAsync(entity);
                _logger.LogInformation("Gift {GiftId} added to basket for user {UserId} with basket id {BasketId}.", basket.GiftId, userId, basket.Id);
                return new ReadBasketDto { Id= basket.Id, amount = basket.amount, GiftId = basket.GiftId, UserId = userId};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding gift {GiftId} to basket for user {UserId}.", basketDto.GiftId, userId);
                throw new Exception(ex.Message);
            }


        }
        
        //update amount
        public async Task<ReadBasketDto?> UpdateBasketAmountAsync(int id, int newAmount)
        {

            if (newAmount < 0 || newAmount > 1000)
            {
                _logger.LogWarning("Attempted to update basket {BasketId} with invalid amount {NewAmount}.", id, newAmount);
                throw new ArgumentOutOfRangeException();
            }
            var basket = await _basketRepository.UpdateBasketAmountAsync(id, newAmount);
            if (basket == null)
            {
                _logger.LogWarning("Basket {BasketId} not found for update.", id);
                return null;
            }
            
            _logger.LogInformation("Basket {BasketId} updated to new amount {NewAmount}.", id, newAmount);
            return Map(basket);
        }
        
        //delete basket
        public async Task<int?> DeleteBasketAsync(int id)
        {
            _logger.LogInformation("Attempting to delete basket {BasketId}.", id);
            var basket = await _basketRepository.DeleteBasketAsync(id);
            
            if(basket == null)
            {
                _logger.LogWarning("Basket {BasketId} not found for deletion.", id);
                return null;
            }

            _logger.LogInformation("Basket {BasketId} deleted successfully.", id);
            return basket.Id;
        }
    }
}

