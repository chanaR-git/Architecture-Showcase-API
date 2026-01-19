using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using projectApiAngular.Repositories;
using System.Data;


namespace Chinese_sale_api.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDonorRepository _donorRepository;
        public GiftService(IGiftRepository repo, IDonorRepository donorRepository,ICategoryRepository categoryRepository)
        {
            _repository = repo;
            _donorRepository = donorRepository;
            _categoryRepository = categoryRepository;
        }

        private static ReadGiftDTO ToReadDto(Gift g) =>
            new ReadGiftDTO
            {
                Id=g.Id,
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImagePath = g.ImagePath,
                CategoryId = g.CategoryId,
                DonorId = g.DonorId,
                CategoryName = g.Category?.Name ?? "",
                DonorName = g.Donor?.Name ?? ""
            };

        public async Task<IEnumerable<ReadGiftDTO>> GetGiftsAsync()
        {
            var gifts = await _repository.GetGiftsAsync();
            return gifts.Select(ToReadDto);
        }

        public async Task<ReadGiftDTO?> AddGiftAsync(CreateGiftDTO g)
        {
            var donor = await _donorRepository.GetDonorByIdAsync((int)g.DonorId);
            if (donor == null)
                throw new ArgumentException("Donor does not exist.");
            
            var category = await _categoryRepository.GetCategoryById((int)g.CategoryId);
            if (category == null)
                throw new ArgumentException("Category does not exist.");
            
            var nameConflict = await _repository.GetGiftByNameAsync(g.Name);
            if (nameConflict != null )
                throw new InvalidOperationException("Gift name already exists.");
            
            Gift newGift = new()
            {
                Name = g.Name,
                Description = g.Description,
                Price = g.Price,
                ImagePath = g.ImagePath,
                CategoryId = g.CategoryId,
                Category = category,
                DonorId = g.DonorId,
                Donor = donor,
                Purchases = new()
            };
                var created = await _repository.AddGiftAsync(newGift);
                return ToReadDto(created);

        }

        public async Task<ReadGiftDTO?> UpdateGiftAsync(string name, UpdateGiftDTO updatedGift)
        {
            var existing = await _repository.GetGiftByNameAsync(name);//name is unique
            if (existing == null) return null;
            if (updatedGift.Name != null)
            {
                var nameConflict = await _repository.GetGiftByNameAsync(updatedGift.Name);
                if (nameConflict != null && nameConflict.Name != name)
                    throw new InvalidOperationException("Gift name already exists.");
            }
            Category? category = null;
            if (updatedGift.CategoryId != null)
            {
                category = await _categoryRepository.GetCategoryById((int)updatedGift.CategoryId);
                if (category == null)
                    throw new ArgumentException("Category does not exist.");
            }


            // preserve existing values for null fields in updatedGift
            existing.Name = updatedGift.Name ?? existing.Name;
            existing.Description = updatedGift.Description ?? existing.Description;
            existing.ImagePath = updatedGift.ImagePath ?? existing.ImagePath;
            existing.Price = updatedGift.Price ?? existing.Price;
            existing.CategoryId = updatedGift.CategoryId ?? existing.CategoryId;
            existing.Category = category ?? existing.Category;
            //existing.Donor = donor ?? existing.Donor;
            //existing.DonorId = updatedGift.DonorId ?? existing.DonorId;
           

            var updated = await _repository.UpdateGiftAsync(existing);
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
            return gifts.Select(ToReadDto);
        }

     
        public async Task<IEnumerable<ReadGiftDTO>> getByNumBuyersAsync(int count)
        {
            var gifts = await _repository.getByNumBuyers(count);
            return gifts.Select(ToReadDto);
        }
    }
}