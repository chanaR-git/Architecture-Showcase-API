using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace Chinese_sale_api.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ChineseSaleDbContext _context;
        private readonly ILogger<BasketRepository> _logger;
        private const string ClassName = nameof(BasketRepository);

        public BasketRepository(ChineseSaleDbContext context, ILogger<BasketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //get a user's basket
        public async Task<IEnumerable<Basket>> GetMyBasketAsync(int idUser)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetMyBasketAsync), ClassName, new { UserId = idUser });
                var baskets = await _context.Baskets
                    .Where(b => b.UserId == idUser)
                    .Include(b => b.User)
                    .Include(b => b.gift).ThenInclude(g=>g.Category)
                    .Include(b=>b.gift).ThenInclude(g=>g.Donor)
                    .ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetMyBasketAsync), ClassName, baskets.Count);
                return baskets;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetMyBasketAsync), ClassName, ex);
                throw;
            }
        }

        //EnterToBasketAsync
        public async Task<Basket> EnterToBasketAsync(Basket basket)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(EnterToBasketAsync), ClassName, new { UserId = basket.UserId, GiftId = basket.GiftId, Amount = basket.amount });
                
                if (!await _context.Gifts.AnyAsync(b => b.Id == basket.GiftId))
                    throw new ArgumentException($"Gift with id {basket.GiftId} does not exist.");

                var existing = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == basket.UserId && b.GiftId == basket.GiftId);
               
                if(existing != null)
                {
                    existing.amount += basket.amount;
                    await _context.SaveChangesAsync();
                    LoggingHelper.LogUpdated(_logger, nameof(EnterToBasketAsync), ClassName, new { BasketId = existing.Id, NewAmount = existing.amount });
                    return existing;
                }

                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
                LoggingHelper.LogCreated(_logger, nameof(EnterToBasketAsync), ClassName, new { basket.Id, basket.UserId, basket.GiftId });
                return basket;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(EnterToBasketAsync), ClassName, ex);
                throw;
            }
        }

        //update amount
        public async Task<Basket?> UpdateBasketAmountAsync(int id, int newAmount)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(UpdateBasketAmountAsync), ClassName, new { BasketId = id, NewAmount = newAmount });
                var basket = await _context.Baskets
                                     .Include(b => b.User).Include(b=>b.gift).ThenInclude(g=>g.Category).Include(b=>b.gift).ThenInclude(g=>g.Donor)
                                     .FirstOrDefaultAsync(b => b.Id == id);
                if (basket == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(UpdateBasketAmountAsync), ClassName, id.ToString());
                    return null;
                }
                basket.amount = newAmount;
                await _context.SaveChangesAsync();
                LoggingHelper.LogUpdated(_logger, nameof(UpdateBasketAmountAsync), ClassName, new { BasketId = id, NewAmount = newAmount });
                return basket;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(UpdateBasketAmountAsync), ClassName, ex);
                throw;
            }
        }

        //delete basket
        public async Task<Basket?> DeleteBasketAsync(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(DeleteBasketAsync), ClassName, new { BasketId = id });
                var basket = await _context.Baskets.FindAsync(id);
                if (basket == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(DeleteBasketAsync), ClassName, id.ToString());
                    return null;
                }
                _context.Baskets.Remove(basket);
                await _context.SaveChangesAsync();
                LoggingHelper.LogDeleted(_logger, nameof(DeleteBasketAsync), ClassName, id.ToString());
                return basket;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(DeleteBasketAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<IDbContextTransaction> beginTransactionAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(beginTransactionAsync), ClassName);
                var transaction = await _context.Database.BeginTransactionAsync();
                LoggingHelper.LogMethodSuccess(_logger, nameof(beginTransactionAsync), ClassName);
                return transaction;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(beginTransactionAsync), ClassName, ex);
                throw;
            }
        }


    }
}
