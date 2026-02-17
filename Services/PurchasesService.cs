using Chinese_sale_api.DTOs;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Exceptions;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chinese_sale_api.Services
{
    public class PurchasesService : IPurchasesService
    {
        private readonly IPurchasesRepository _repo;
        private readonly ILogger<PurchasesService> _logger;
        private readonly IGiftRepository _giftRepository;
        private const string ClassName = nameof(PurchasesService);

        public PurchasesService(IPurchasesRepository repo, IGiftRepository giftRepository, ILogger<PurchasesService> logger)
        {
            _repo = repo;
            _giftRepository = giftRepository;
            _logger = logger;
        }

        private static ReadPurchaseDto Map(Purchase p) =>
            new ReadPurchaseDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.Name,
                CustomerEmail = p.Customer.Email,
                GiftId = p.GiftId,
                GiftName = p.Gift.Name,
                GiftPrice = p.Gift.Price,
                PurchDate = p.PurchDate
            };

        public async Task<ReadPurchaseDto> AddPurchaseAsync(CreatePurchaseDto dto)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(AddPurchaseAsync), ClassName, new { CustomerId = dto.CustomerId, GiftId = dto.GiftId });
            
            if (dto.PurchDate > DateTime.Now)
            {
                LoggingHelper.LogValidationError(_logger, nameof(AddPurchaseAsync), ClassName, "Purchase date cannot be in the future");
                throw new ArgumentException("Purchase date cannot be in the future.");
            }

            var gift = await _giftRepository.GetGiftByIdAsync(dto.GiftId);
            if (gift != null && gift.WinnerId != null)
            {
                var winnerName = gift.Winner?.Name;
                LoggingHelper.LogDuplicateAttempt(_logger, nameof(AddPurchaseAsync), ClassName, $"GiftId: {dto.GiftId}");
                throw new GiftAlreadyAsignedException(winnerName);
            }

            var entity = new Purchase
            {
                CustomerId = dto.CustomerId,
                GiftId = dto.GiftId,
                PurchDate = dto.PurchDate
            };
            try
            {
                var saved = await _repo.AddPurchaseAsync(entity);
                LoggingHelper.LogCreated(_logger, nameof(AddPurchaseAsync), ClassName, new { saved.Id, saved.CustomerId, saved.GiftId });
                return Map(saved);
            }
            catch (ArgumentException ex)
            {
                LoggingHelper.LogValidationError(_logger, nameof(AddPurchaseAsync), ClassName, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(AddPurchaseAsync), ClassName, ex);
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetBuyersDetailsAsync()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetBuyersDetailsAsync), ClassName);
            var items = await _repo.GetBuyersDetailsAsync();
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetBuyersDetailsAsync), ClassName, items.Count());
            return items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesByGiftAsync(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesByGiftAsync), ClassName, new { GiftName = name });
            var items = await _repo.GetPurchasesByGiftAsync(name);
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesByGiftAsync), ClassName, items.Count());
            return items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedBySellingsAsync()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesSortedBySellingsAsync), ClassName);
            var items = await _repo.GetPurchasesSortedBySellingsAsync();
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesSortedBySellingsAsync), ClassName, items.Count());
            return items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedByPriceAsync()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetPurchasesSortedByPriceAsync), ClassName);
            var items = await _repo.GetPurchasesSortedByPriceAsync();
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetPurchasesSortedByPriceAsync), ClassName, items.Count());
            return items.Select(Map);
        }

        public async Task<decimal> GetTotalSalesRevenue()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetTotalSalesRevenue), ClassName);
            var purchases = await _repo.GetAllPurchasesAsync();
            
            if (purchases == null || !purchases.Any())
            {
                LoggingHelper.LogValidationError(_logger, nameof(GetTotalSalesRevenue), ClassName, "No purchases found");
                return 0;
            }

            decimal totalRevenue = purchases.Sum(purchase => purchase.Gift.Price);
            LoggingHelper.LogMethodSuccess(_logger, nameof(GetTotalSalesRevenue), ClassName, new { TotalRevenue = totalRevenue });
            return totalRevenue;
        }
    }
}