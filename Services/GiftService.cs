using Chinese_sale_api.Data;
using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Chinese_sale_api.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _repository;
        public GiftService(IGiftRepository repo)
        {
            _repository = repo;
        }

        private static ReadGiftDTO ToReadDto(Gift g) =>
            new ReadGiftDTO
            {
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImagePath = g.ImagePath,
                CategoryName = g.Category.Name,
                DonorName = g.Donor.Name
            };

        public async Task<IEnumerable<ReadGiftDTO>> GetGiftsAsync()
        {
            var gifts = await _repository.GetGiftsAsync();
            return gifts is null ? Enumerable.Empty<ReadGiftDTO>()
                                 : gifts.Select(ToReadDto);
        }

        public async Task<ReadGiftDTO?> AddGiftAsync(CreateGiftDTO g)
        {
            Gift newGift = new()
            {
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImagePath = g.ImagePath,
                CategoryId = g.CategoryId,
                DonorId = g.DonorId,
                Purchases = new()
            };
            try
            {
                var created = await _repository.AddGiftAsync(newGift);
                return ToReadDto(created);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReadGiftDTO?> UpdateGiftAsync(string name, UpdateGiftDTO updatedGift)
        {
            var existing = await _repository.GetGiftByNameAsync(name);//name is unique
            if (existing == null) return null;

            // preserve existing values for null fields in updatedGift
            var updatedEntity = new Gift()
            {
                Name = updatedGift.Name ?? existing.Name,
                Description = updatedGift.Description ?? existing.Description,
                ImagePath = updatedGift.ImagePath ?? existing.ImagePath,
                Price = updatedGift.Price ?? existing.Price,
                CategoryId = updatedGift.CategoryId ?? existing.CategoryId,
                DonorId = updatedGift.DonorId ?? existing.DonorId
            };

            var updated = await _repository.UpdateGiftAsync(existing.Name, updatedEntity);
            return updated is null ? null : ToReadDto(updated);
        }
        public async Task<ReadGiftDTO?> DeleteGiftAsync(string name)
        {
            var gift = await _repository.DeleteGiftAsync(name);
            return gift is null ? null : ToReadDto(gift);
        }

        public async Task<ReadGiftDTO?> GetGiftByNameAsync(string name)
        {
            var gift = await _repository.GetGiftByNameAsync(name);
            return gift is null ? null : ToReadDto(gift);
        }

        public async Task<IEnumerable<ReadGiftDTO>?> GetGiftByDonorAsync(string name)
        {
            var gifts = await _repository.GetGiftByDonorAsync(name);
            return (gifts is null || gifts.Count() == 0) ? null : gifts.Select(ToReadDto);
        }

     
        public async Task<IEnumerable<ReadGiftDTO>> getByNumBuyersAsync(int count)
        {
            var gifts = await _repository.getByNumBuyers(count);
            return (gifts is null || gifts.Count() == 0) ? Enumerable.Empty<ReadGiftDTO>() : gifts.Select(ToReadDto);
        }
    }
}