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
        //private readonly CheineseSale_DBContext _context;
        public BasketService(IBasketRepository basketRepository, IHttpContextAccessor httpContextAccessor)
        {
            _basketRepository = basketRepository;
            _httpContextAccessor = httpContextAccessor;
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
                }
            };
        }

        //get current user id
        private int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException();

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("User Id claim missing");

            return int.Parse(userIdClaim.Value);
        }

        //get the current user's basket items   
        public async Task<IEnumerable<ReadBasketDto>> GetMyBasket()
        {
            int userId = GetCurrentUserId();
            var baskets = await _basketRepository.GetMyBasketAsync(userId);
            return baskets.Select(Map);
        }

        //EnterToBasketAsync
        public async Task<ReadBasketDto> EnterToBasketAsync(CreateBasketDto basketDto)
        {
            int userId = GetCurrentUserId();

            try
            {
                var entity = new Basket
                {

                    UserId = userId,
                    GiftId = basketDto.GiftId,
                    amount = basketDto.amount,

                };

                var basket = await _basketRepository.EnterToBasketAsync(entity);
                return Map(basket);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
        //update amount
        public async Task<ReadBasketDto?> UpdateBasketAmountAsync(int id, int newAmount)
        {
            if (newAmount < 0 || newAmount > 1000)
                throw new ArgumentOutOfRangeException();
            var basket = await _basketRepository.UpdateBasketAmountAsync(id, newAmount);
            if (basket == null)
            {
                return null;
            }
            return Map(basket);
        }
        //delete basket
        public async Task<ReadBasketDto?> DeleteBasketAsync(int id)
        {
            var basket = await _basketRepository.DeleteBasketAsync(id);

            return basket is null ? null : Map(basket);
        }
    }
}

