using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Microsoft.OpenApi.Extensions;
using projectApiAngular.Repositories;
using System.Data;

namespace Chinese_sale_api.Services
{
    public class BasketService
    {
        private readonly IBasketRepository _basketRepository;
        public BasketService(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
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
                    Id = b.User.Id,
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


        //get all basket items   
        public async Task<IEnumerable<ReadBasketDto>> GetAllBasketsAsync()
        {
            var basket = await _basketRepository.GetAllBasketsAsync();
            return basket.Select(Map);
        }
        //EnterToBasketAsync
        public async Task<ReadBasketDto> EnterToBasketAsync(CreateBasketDto basketDto)
        {
            try
            {
                var entity = new Basket
                {

                    UserId = basketDto.UserId,
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
            var basket = await _basketRepository.UpdateBasketAmountAsync(id, newAmount);
            if (basket == null)
            {
                return null;
            }
            return Map(basket);
        }
        //delete basket
    }
}

