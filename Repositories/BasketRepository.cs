using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace projectApiAngular.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ChineseSaleDbContext _context;
        public BasketRepository(ChineseSaleDbContext context)
        {
            _context = context;
        }

        //get a user's basket
        public async Task<IEnumerable<Basket>> GetMyBasketAsync(int idUser)
        {
            return await _context.Baskets
                .Where(b => b.UserId == idUser)
                .Include(b => b.User)
                .Include(b => b.gift).ThenInclude(g=>g.Category)
                .Include(b=>b.gift).ThenInclude(g=>g.Donor)
                .ToListAsync();
        }

        //EnterToBasketAsync
        public async Task<Basket> EnterToBasketAsync(Basket basket)
        {
            if (!await _context.Gifts.AnyAsync(b => b.Id == basket.GiftId))
                throw new ArgumentException($"Gift with id {basket.GiftId} does not exist.");

            var existing = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == basket.UserId && b.GiftId == basket.GiftId);
           
            if(existing != null)
            {
                existing.amount += basket.amount;
                await _context.SaveChangesAsync();
                return existing;
            }

            _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
                return basket;
        }

        //update amount
        public async Task<Basket?> UpdateBasketAmountAsync(int id, int newAmount)
        {
            var basket = await _context.Baskets
                                 .Include(b => b.User).Include(b=>b.gift).ThenInclude(g=>g.Category).Include(b=>b.gift).ThenInclude(g=>g.Donor)    // כולל את המשתמש
                                 .FirstOrDefaultAsync(b => b.Id == id);
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

        public async Task<IDbContextTransaction> beginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }


    }
}
