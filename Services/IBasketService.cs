using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface IBasketService
    {
        Task<ReadBasketDto?> DeleteBasketAsync(int id);
        Task<ReadBasketDto> EnterToBasketAsync(CreateBasketDto basketDto);
        Task<IEnumerable<ReadBasketDto>> GetMyBasket();
        Task<ReadBasketDto?> UpdateBasketAmountAsync(int id, int newAmount);
    }
}