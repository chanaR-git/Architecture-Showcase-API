using Chinese_sale_api.DTO;
using Microsoft.EntityFrameworkCore.Storage;

namespace Chinese_sale_api.Services
{
    public interface IBasketService
    {
        Task<bool> BuyAllBasketsAsync();
        Task<int?> DeleteBasketAsync(int id);
        Task<ReadBasketDto?> EnterToBasketAsync(CreateBasketDto basketDto);
        Task<IEnumerable<ReadBasketDto>> GetMyBasket();
        Task<ReadBasketDto?> UpdateBasketAmountAsync(int id, int newAmount);


    }
}