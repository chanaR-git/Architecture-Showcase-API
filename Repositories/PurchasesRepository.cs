using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Repositories
{
    public class PurchasesRepository : IPurchasesRepository
    {
        private readonly ChineseSaleDbContext _context;
        public PurchasesRepository(ChineseSaleDbContext cntx)
        {
            _context = cntx;
        }
        //get by gift name
        public async Task<IEnumerable<Purchase>> GetPurchasesByGiftAsync(string name)
        {
            var purchases = await _context.Purchases.Include(p => p.Gift).Where(p => p.Gift.Name.Equals(name)).ToListAsync();
            return purchases;
        }

        //get buyers details
        public async Task<IEnumerable<Purchase>> GetBuyersDetailsAsync()
        {
            var buyers = await _context.Purchases.Include(p => p.Customer).Include(p=>p.Gift).ToListAsync();
            return buyers;
        }

        //sort by sellings
        public async Task<IEnumerable<Purchase>> GetPurchasesSortedBySellingsAsync()
        {
            var sortedPurchases = await _context.Purchases
                .GroupBy(p => p.GiftId)
                .Select(g => new
                {
                    GiftId = g.Key,
                    Count = g.Count(),
                    Purchases = g.Select(p => p) // לוקח את הפרטים מהרכישה
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync(); // ממשיכים ל-ToListAsync כאן
                
            //  להוציא את רכישות מהקבוצות
            var purchasesList = sortedPurchases.SelectMany(g => g.Purchases);

            var finalPurchases = await _context.Purchases
                                    .Where(p => purchasesList.Select(x => x.Id).Contains(p.Id))
                                    .Include(p => p.Gift)
                                    .Include(p => p.Customer)
                                    .ToListAsync();

            return finalPurchases;
        }
        //sort by price
        public async Task<IEnumerable<Purchase>> GetPurchasesSortedByPriceAsync()
        {
            var sortedPurchases = await _context.Purchases
                                        .Include(p => p.Gift)
                                        .Include(p => p.Customer)
                                        .OrderByDescending(p => p.Gift.Price)
                                        .ToListAsync();
            return sortedPurchases;
        }


        //add purchase
        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
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
