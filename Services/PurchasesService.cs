using Chinese_sale_api.DTOs;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Exceptions;
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
            if (dto.PurchDate > DateTime.Now)
                throw new ArgumentException("Purchase date cannot be in the future.");

            var gift = await _giftRepository.GetGiftByIdAsync(dto.GiftId);
            if (gift != null && gift.WinnerId != null)
            {
                var winnerName = gift.Winner?.Name;
                _logger.LogWarning("Attempted to add purchase for gift {GiftId} but it already has a winner: {Winner}.", dto.GiftId, winnerName);
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
                return Map(saved);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetBuyersDetailsAsync()
        {
            var items = await _repo.GetBuyersDetailsAsync();
            return  items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesByGiftAsync(string name)
        {
            var items = await _repo.GetPurchasesByGiftAsync(name);
            return items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedBySellingsAsync()
        {
            var items = await _repo.GetPurchasesSortedBySellingsAsync();
            return items.Select(Map);
        }

        public async Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedByPriceAsync()
        {
            var items = await _repo.GetPurchasesSortedByPriceAsync();
            return items.Select(Map);
        }

        public async Task<decimal> GetTotalSalesRevenue()
        {
            _logger.LogInformation("Calculating total sales revenue.");

            var purchases = await _repo.GetAllPurchasesAsync(); 
            if (purchases == null || !purchases.Any())
            {
                _logger.LogWarning("No purchases found.");
                return 0;
            }

            decimal totalRevenue = purchases.Sum(purchase => purchase.Gift.Price);  
            return totalRevenue;
        }
    }
}