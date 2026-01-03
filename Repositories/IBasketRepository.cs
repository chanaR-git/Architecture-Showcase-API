using Chinese_sale_api.Models;

namespace projectApiAngular.Repositories
{
    public interface IBasketRepository
    {
        Task<Basket?> DeleteBasketAsync(int id);
        Task<Basket> EnterToBasketAsync(Basket basket);
        Task<IEnumerable<Basket>> GetMyBasketAsync(int idUser);
        Task<Basket?> UpdateBasketAmountAsync(int id, int newAmount);
    }
}