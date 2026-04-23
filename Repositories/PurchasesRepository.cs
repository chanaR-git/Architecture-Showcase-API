using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Repositories
{
    public class PurchasesRepository : IPurchasesRepository
    {
        private readonly ChineseSaleDbContext _context;
        private readonly ILogger<PurchasesRepository> _logger;
        private const string ClassName = nameof(PurchasesRepository);

        public PurchasesRepository(ChineseSaleDbContext cntx, ILogger<PurchasesRepository> logger)
        {
            _context = cntx;
            _logger = logger;
        }
        //get all purchases
        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetAllPurchasesAsync), ClassName);
                var purchases = await _context.Purchases.Include(p => p.Gift).Include(p => p.Customer).ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetAllPurchasesAsync), ClassName, purchases.Count);
                return purchases;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetAllPurchasesAsync), ClassName, ex);
                throw;
            }
        }

        //get by gift name
        public async Task<IEnumerable<Purchase>> GetPurchasesByGiftAsync(string name)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesByGiftAsync), ClassName, new { GiftName = name });
                var purchases = await _context.Purchases.Include(p => p.Gift).Where(p => p.Gift.Name.Equals(name)).ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesByGiftAsync), ClassName, purchases.Count);
                return purchases;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetPurchasesByGiftAsync), ClassName, ex);
                throw;
            }
        }


        //get buyers details
        public async Task<IEnumerable<Purchase>> GetBuyersDetailsAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetBuyersDetailsAsync), ClassName);
                var buyers = await _context.Purchases.Include(p => p.Customer).Include(p=>p.Gift).ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetBuyersDetailsAsync), ClassName, buyers.Count);
                return buyers;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetBuyersDetailsAsync), ClassName, ex);
                throw;
            }
        }

        //sort by sellings
        public async Task<IEnumerable<Purchase>> GetPurchasesSortedBySellingsAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesSortedBySellingsAsync), ClassName);
                var sortedPurchases = await _context.Purchases
                    .GroupBy(p => p.GiftId)
                    .Select(g => new
                    {
                        GiftId = g.Key,
                        Count = g.Count(),
                        Purchases = g.Select(p => p)
                    })
                    .OrderByDescending(g => g.Count)
                    .ToListAsync();
                    
                var purchasesList = sortedPurchases.SelectMany(g => g.Purchases);

                var finalPurchases = await _context.Purchases
                                        .Where(p => purchasesList.Select(x => x.Id).Contains(p.Id))
                                        .Include(p => p.Gift)
                                        .Include(p => p.Customer)
                                        .ToListAsync();

                LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesSortedBySellingsAsync), ClassName, finalPurchases.Count);
                return finalPurchases;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetPurchasesSortedBySellingsAsync), ClassName, ex);
                throw;
            }
        }

        //sort by price
        public async Task<IEnumerable<Purchase>> GetPurchasesSortedByPriceAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesSortedByPriceAsync), ClassName);
                var sortedPurchases = await _context.Purchases
                                            .Include(p => p.Gift)
                                            .Include(p => p.Customer)
                                            .OrderByDescending(p => p.Gift.Price)
                                            .ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesSortedByPriceAsync), ClassName, sortedPurchases.Count);
                return sortedPurchases;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetPurchasesSortedByPriceAsync), ClassName, ex);
                throw;
            }
        }


        //add purchase
        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(AddPurchaseAsync), ClassName, new { CustomerId = purchase.CustomerId, GiftId = purchase.GiftId });
                
                // Validate foreign keys to avoid FK constraint errors
                if (!await _context.Users.AnyAsync(u => u.Id == purchase.CustomerId))
                    throw new ArgumentException($"no such customer :(");
                if (!await _context.Gifts.AnyAsync(g => g.Id == purchase.GiftId))
                    throw new ArgumentException($"no such gift :(");

                _context.Purchases.Add(purchase);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    LoggingHelper.LogDatabaseError(_logger, nameof(AddPurchaseAsync), ClassName, ex);
                    throw new InvalidOperationException("Error saving Purchase to database. See inner exception for details.", ex);
                }
                
                // Ensure navigation properties are loaded before returning
                await _context.Entry(purchase).Reference(p => p.Gift).LoadAsync();
                await _context.Entry(purchase).Reference(p => p.Customer).LoadAsync();
                
                LoggingHelper.LogCreated(_logger, nameof(AddPurchaseAsync), ClassName, new { purchase.Id, CustomerId = purchase.CustomerId, GiftId = purchase.GiftId });
                return purchase;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(AddPurchaseAsync), ClassName, ex);
                throw;
            }
        }
    }
}
