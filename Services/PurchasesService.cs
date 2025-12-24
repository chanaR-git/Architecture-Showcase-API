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
    }
}