using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Repositories
{
    public class PurchasesRepository
    {
        private readonly CheineseSale_DBContext _context;
        public PurchasesRepository(CheineseSale_DBContext cntx)
        {
            _context = cntx;
        }

        public async Task<IEnumerable<Purchase>> GetPurchasesByGiftAsync(string name)
        {
            var purchases = await _context.Purchases.Include(p => p.Gift).Where(p => p.Gift.Name.Equals(name)).ToListAsync();
            return purchases;
        }

        public async Task<IEnumerable<Purchase>> GetBuyersDetailsAsync()
        {
            var buyers = await _context.Purchases.Include(p => p.Customer).ToListAsync();
            return buyers;
        }
        public async Task<IEnumerable<Purchase>> BestSellersAsync(Purchase purchase)
        {
            var counts = await _context.Purchases
                                .GroupBy(p => p.GiftId)
                                .Select(g => new { GiftId = g.Key, Count = g.Count() })
                                .ToListAsync();

            if (counts.Count == 0)
                return Enumerable.Empty<Purchase>();

            // Determine max count and the gift ids that have that count
            var maxCount = counts.Max(c => c.Count);
            var bestGiftIds = counts.Where(c => c.Count == maxCount).Select(c => c.GiftId).ToList();

            // Return all purchases for those best-selling gifts. Include navigation properties as needed.
            var purchases = await _context.Purchases
                .Include(p => p.Gift)
                .Include(p => p.Customer)
                .Where(p => bestGiftIds.Contains(p.GiftId))
                .ToListAsync();

            return purchases;
    }
        //sort by sellings
       public async Task<IEnumerable<Purchase>> GetPurchasesSortedBySellingsAsync()
        {
            var grpsGiftsByCount = await _context.Purchases
                                .GroupBy(p => p.GiftId)
                                .Select(g=> new { GiftId = g.Key, Count = g.Count()})
                                .OrderByDescending(g=>g.Count)
                                .ToListAsync();

            return purchases;
        }
        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
    {
        var purchases = await _context.Purchases
                            .Include(p => p.Gift)
                            .Include(p => p.Customer)
                            
                                .ToListAsync();
            return purchases;
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            _context.Purchases.Add(purchase);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // preserve inner exception for diagnostics
                throw new InvalidOperationException("Error saving Purchase to database. See inner exception for details.", ex);
            }
            // Ensure navigation properties are loaded before returning
            await _context.Entry(purchase).Reference(p => p.Gift).LoadAsync();
            await _context.Entry(purchase).Reference(p => p.Customer).LoadAsync();
            return purchase;
        }
    }
}
