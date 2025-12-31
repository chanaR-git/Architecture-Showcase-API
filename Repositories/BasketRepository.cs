using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;


namespace projectApiAngular.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly CheineseSale_DBContext _context;
        public BasketRepository(CheineseSale_DBContext context)
        {
            _context = context;
        }

        //get all baskets
        public async Task<IEnumerable<Basket>> GetAllBasketsAsync()
        {
            return await _context.Baskets
                .Include(b => b.User)
                .Include(b => b.gift)
                .ToListAsync();
        }

        //EnterToBasketAsync
        public async Task<Basket> EnterToBasketAsync(Basket basket)
        {
            if (!await _context.Gifts.AnyAsync(b => b.Id == basket.GiftId))
                throw new ArgumentException($"Gift with id {basket.GiftId} does not exist.");

            if (!await _context.Users.AnyAsync(c => c.Id == basket.UserId))
                throw new ArgumentException($"User with id {basket.UserId} does not exist.");
            try
            {
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
                return basket;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while proccing the Basket.", ex);
            }


        }

        //update amount
        public async Task<Basket?> UpdateBasketAmountAsync(int id, int newAmount)
        {
            var basket = await _context.Baskets.FindAsync(id);
            if (basket == null)
            {
                return null;
            }
            basket.amount = newAmount;
            await _context.SaveChangesAsync();
            return basket;
        }
        //delete basket
        public async Task<Basket?> DeleteBasketAsync(int id)
        {
            var basket = await _context.Baskets.FindAsync(id);
            if (basket == null)
            {
                return null;
            }
            _context.Baskets.Remove(basket);
            await _context.SaveChangesAsync();
            return basket;
        }


    }
}
