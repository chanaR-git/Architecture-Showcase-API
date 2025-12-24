using Chinese_sale_api.DTOs;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
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
        public PurchasesService(IPurchasesRepository repo)
        {
            _repo = repo;
        }

        private static PurchaseDto Map(Purchase p) =>
            new PurchaseDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer?.Name,
                CustomerEmail = p.Customer?.Email,
                GiftId = p.GiftId,
                GiftName = p.Gift?.Name,
                GiftPrice = p.Gift?.Price,
                PurchDate = p.PurchDate
            };

        public async Task<PurchaseDto> AddPurchaseAsync(CreatePurchaseDto dto)
        {
            var entity = new Purchase
            {
                CustomerId = dto.CustomerId,
                GiftId = dto.GiftId,
                PurchDate = dto.PurchDate ?? DateTime.UtcNow
            };

            var saved = await _repo.AddPurchaseAsync(entity);
            return Map(saved);
        }

        public async Task<IEnumerable<PurchaseDto>> GetBuyersDetailsAsync()
        {
            var items = await _repo.GetBuyersDetailsAsync();
            return items.Select(Map);
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesByGiftAsync(string name)
        {
            var items = await _repo.GetPurchasesByGiftAsync(name);
            return items.Select(Map);
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesSortedBySellingsAsync()
        {
            var items = await _repo.GetPurchasesSortedBySellingsAsync();
            return items.Select(Map);
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesSortedByPriceAsync()
        {
            var items = await _repo.GetPurchasesSortedByPriceAsync();
            return items.Select(Map);
        }
    }
}