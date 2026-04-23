using Chinese_sale_api.Controllers;
using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Chinese_sale_api.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly ChineseSaleDbContext _context;
        private readonly ILogger<GiftRepository> _logger;
        private const string ClassName = nameof(GiftRepository);

        public GiftRepository(ChineseSaleDbContext context, ILogger<GiftRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<Gift>> GetGiftsAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetGiftsAsync), ClassName);
                var gifts = await _context.Gifts.AsNoTracking().Include(g => g.Category).Include(g => g.Donor).ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetGiftsAsync), ClassName, gifts.Count);
                return gifts;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetGiftsAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<Gift> AddGiftAsync(Gift gift)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(AddGiftAsync), ClassName, new { gift.Name });
                _context.Gifts.Add(gift);
                await _context.SaveChangesAsync();
                LoggingHelper.LogCreated(_logger, nameof(AddGiftAsync), ClassName, new { gift.Id, gift.Name });
                return gift;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(AddGiftAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<Gift?> UpdateGiftAsync(Gift gift)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(UpdateGiftAsync), ClassName, new { gift.Id });
                await _context.SaveChangesAsync();
                LoggingHelper.LogUpdated(_logger, nameof(UpdateGiftAsync), ClassName, new { gift.Id, gift.Name });
                return gift;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(UpdateGiftAsync), ClassName, ex);
                throw;
            }
        }

        //update gift winner
        public async Task<User?> UpdateGiftWinnerAsync(string name, int winnerId)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(UpdateGiftWinnerAsync), ClassName, new { GiftName = name, WinnerId = winnerId });
                var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Name == name);
                if (gift == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(UpdateGiftWinnerAsync), ClassName, name);
                    return null;
                }

                if (gift.WinnerId != null)
                {
                    LoggingHelper.LogDuplicateAttempt(_logger, nameof(UpdateGiftWinnerAsync), ClassName, $"Gift: {name}");
                    throw new InvalidOperationException("It's immposible to do duplicate lotteries in one sale :(");
                }

                gift.WinnerId = winnerId;
                await _context.SaveChangesAsync();
                var winner = await _context.Users.FindAsync(winnerId);
                LoggingHelper.LogUpdated(_logger, nameof(UpdateGiftWinnerAsync), ClassName, new { GiftName = name, WinnerName = winner?.Name });
                return winner;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(UpdateGiftWinnerAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<Gift?> DeleteGiftAsync(string name)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(DeleteGiftAsync), ClassName, new { GiftName = name });
                var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Name == name);
                if (gift == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(DeleteGiftAsync), ClassName, name);
                    return null;
                }
                _context.Gifts.Remove(gift);
                await _context.SaveChangesAsync();
                LoggingHelper.LogDeleted(_logger, nameof(DeleteGiftAsync), ClassName, name);
                return gift;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(DeleteGiftAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<Gift?> GetGiftByNameAsync(string name)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetGiftByNameAsync), ClassName, new { GiftName = name });
                var gift = await _context.Gifts
                            .Include(g => g.Category)
                            .Include(g => g.Donor)
                            .Include(g => g.Winner)
                            .FirstOrDefaultAsync(d => d.Name == name);
                if (gift == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetGiftByNameAsync), ClassName, name);
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftByNameAsync), ClassName);
                return gift;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetGiftByNameAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<Gift?> GetGiftByIdAsync(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetGiftByIdAsync), ClassName, new { GiftId = id });
                var gift = await _context.Gifts
                            .Include(g => g.Category)
                            .Include(g => g.Donor)
                            .Include(g => g.Winner)
                            .FirstOrDefaultAsync(d => d.Id == id);
                if (gift == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetGiftByIdAsync), ClassName, id.ToString());
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftByIdAsync), ClassName);
                return gift;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetGiftByIdAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<IEnumerable<Gift>> GetGiftByDonorAsync(string name)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetGiftByDonorAsync), ClassName, new { DonorName = name });
                var gifts = await _context.Gifts
                               .Include(g => g.Donor)
                               .Where(g => g.Donor.Name == name)
                               .ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetGiftByDonorAsync), ClassName, gifts.Count);
                return gifts;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetGiftByDonorAsync), ClassName, ex);
                throw;
            }
        }
        public async Task<IEnumerable<Gift>> getByNumBuyers(int count)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(getByNumBuyers), ClassName, new { BuyerCount = count });
                var gifts = await _context.Gifts
                            .Include(g => g.Purchases)
                            .Where(g => g.Purchases != null && g.Purchases.Count == count)
                            .ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(getByNumBuyers), ClassName, gifts.Count);
                return gifts;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(getByNumBuyers), ClassName, ex);
                throw;
            }
        }

        //start a new chinese sale
        public async Task<int?> StartNewChineseSaleAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(StartNewChineseSaleAsync), ClassName);
                var gifts = await _context.Gifts.ToListAsync();
                foreach (var gift in gifts)
                {
                    gift.WinnerId = null;
                }
                await _context.SaveChangesAsync();
                LoggingHelper.LogMethodSuccess(_logger, nameof(StartNewChineseSaleAsync), ClassName, new { ResetGiftCount = gifts.Count });
                return null;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(StartNewChineseSaleAsync), ClassName, ex);
                throw;
            }
        }

        //get all gifts using pagination
        public async Task<(IEnumerable<Gift> Items, int TotalCount)> GetGiftsPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetGiftsPagedAsync), ClassName, new { PageNumber = pageNumber, PageSize = pageSize });
                var query = _context.Gifts
                    .AsNoTracking()
                    .Include(g => g.Category)
                    .Include(g => g.Donor);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(g => g.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftsPagedAsync), ClassName, new { ItemsCount = items.Count, TotalCount = totalCount });
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetGiftsPagedAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<User?> GetWinnerOfGift(string giftName)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetWinnerOfGift), ClassName, new { GiftName = giftName });
                var gift = await _context.Gifts
                            .Include(g => g.Winner)
                            .FirstOrDefaultAsync(g => g.Name == giftName);
                if (gift?.Winner == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetWinnerOfGift), ClassName, giftName);
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetWinnerOfGift), ClassName, new { WinnerName = gift.Winner.Name });
                return gift?.Winner;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetWinnerOfGift), ClassName, ex);
                throw;
            }
        }
    }
}