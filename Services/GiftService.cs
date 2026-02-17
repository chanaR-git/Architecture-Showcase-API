using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Utilities;
using projectApiAngular.Repositories;
using System.Data;


namespace Chinese_sale_api.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<GiftService> _logger;
        private const string ClassName = nameof(GiftService);

        public GiftService(IGiftRepository repo, IDonorRepository donorRepository, ICategoryRepository categoryRepository, ILogger<GiftService> logger)
        {
            _repository = repo;
            _donorRepository = donorRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        private static ReadGiftDTO ToReadDto(Gift g) =>
            new ReadGiftDTO
            {
                Id = g.Id,
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
            LoggingHelper.LogMethodStart(_logger, nameof(GetGiftsAsync), ClassName);
            var gifts = await _repository.GetGiftsAsync();
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetGiftsAsync), ClassName, gifts.Count());
            return gifts.Select(ToReadDto);
        }

        public async Task<ReadGiftDTO?> AddGiftAsync(CreateGiftDTO g)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(AddGiftAsync), ClassName, new { g.Name });
            
            var donor = await _donorRepository.GetDonorByIdAsync((int)g.DonorId);
            if (donor == null)
            {
                LoggingHelper.LogValidationError(_logger, nameof(AddGiftAsync), ClassName, "Donor does not exist");
                throw new ArgumentException("Donor does not exist.");
            }

            var category = await _categoryRepository.GetCategoryById((int)g.CategoryId);
            if (category == null)
            {
                LoggingHelper.LogValidationError(_logger, nameof(AddGiftAsync), ClassName, "Category does not exist");
                throw new ArgumentException("Category does not exist.");
            }

            var nameConflict = await _repository.GetGiftByNameAsync(g.Name);
            if (nameConflict != null)
            {
                LoggingHelper.LogDuplicateAttempt(_logger, nameof(AddGiftAsync), ClassName, g.Name);
                throw new InvalidOperationException("Gift name already exists.");
            }

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
            LoggingHelper.LogCreated(_logger, nameof(AddGiftAsync), ClassName, new { created.Id, created.Name });
            return ToReadDto(created);
        }

        public async Task<ReadGiftDTO?> UpdateGiftAsync(string name, UpdateGiftDTO updatedGift)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(UpdateGiftAsync), ClassName, new { GiftName = name });
            
            var existing = await _repository.GetGiftByNameAsync(name);
            if (existing == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(UpdateGiftAsync), ClassName, name);
                return null;
            }
            
            if (updatedGift.Name != null)
            {
                var nameConflict = await _repository.GetGiftByNameAsync(updatedGift.Name);
                if (nameConflict != null && nameConflict.Name != name)
                {
                    LoggingHelper.LogDuplicateAttempt(_logger, nameof(UpdateGiftAsync), ClassName, updatedGift.Name);
                    throw new InvalidOperationException("Gift name already exists.");
                }
            }
            
            Category? category = null;
            if (updatedGift.CategoryId != null)
            {
                category = await _categoryRepository.GetCategoryById((int)updatedGift.CategoryId);
                if (category == null)
                {
                    LoggingHelper.LogValidationError(_logger, nameof(UpdateGiftAsync), ClassName, "Category does not exist");
                    throw new ArgumentException("Category does not exist.");
                }
            }

            existing.Name = updatedGift.Name ?? existing.Name;
            existing.Description = updatedGift.Description ?? existing.Description;
            existing.ImagePath = updatedGift.ImagePath ?? existing.ImagePath;
            existing.Price = updatedGift.Price ?? existing.Price;
            existing.CategoryId = updatedGift.CategoryId ?? existing.CategoryId;
            existing.Category = category ?? existing.Category;

            var updated = await _repository.UpdateGiftAsync(existing);
            LoggingHelper.LogUpdated(_logger, nameof(UpdateGiftAsync), ClassName, new { updated.Id, updated.Name });
            return updated is null ? null : ToReadDto(updated);
        }

        public async Task<ReadGiftDTO?> DeleteGiftAsync(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(DeleteGiftAsync), ClassName, new { GiftName = name });
            var gift = await _repository.DeleteGiftAsync(name);
            if (gift is null)
                LoggingHelper.LogNotFound(_logger, nameof(DeleteGiftAsync), ClassName, name);
            else
                LoggingHelper.LogDeleted(_logger, nameof(DeleteGiftAsync), ClassName, name);
            return gift is null ? null : ToReadDto(gift);
        }

        public async Task<ReadGiftDTO?> GetGiftByNameAsync(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetGiftByNameAsync), ClassName, new { GiftName = name });
            var gift = await _repository.GetGiftByNameAsync(name);
            if (gift is null)
                LoggingHelper.LogNotFound(_logger, nameof(GetGiftByNameAsync), ClassName, name);
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftByNameAsync), ClassName);
            return gift is null ? null : ToReadDto(gift);
        }

        public async Task<IEnumerable<ReadGiftDTO>?> GetGiftByDonorAsync(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetGiftByDonorAsync), ClassName, new { DonorName = name });
            var gifts = await _repository.GetGiftByDonorAsync(name);
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetGiftByDonorAsync), ClassName, gifts.Count());
            return gifts.Select(ToReadDto);
        }

        public async Task<IEnumerable<ReadGiftDTO>> getByNumBuyersAsync(int count)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(getByNumBuyersAsync), ClassName, new { BuyerCount = count });
            var gifts = await _repository.getByNumBuyers(count);
            LoggingHelper.LogMethodWithCount(_logger, nameof(getByNumBuyersAsync), ClassName, gifts.Count());
            return gifts.Select(ToReadDto);
        }

        public async Task<PagedResult<ReadGiftDTO>> GetGiftsPagedAsync(PaginationParams @params)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetGiftsPagedAsync), ClassName, @params);
            var (items, totalCount) = await _repository.GetGiftsPagedAsync(@params.PageNumber, @params.PageSize);

            var result = new PagedResult<ReadGiftDTO>
            {
                Items = items.Select(ToReadDto),
                TotalCount = totalCount
            };
            LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftsPagedAsync), ClassName, new { ItemsCount = items.Count(), TotalCount = totalCount });
            return result;
        }

        public async Task<string?> GetWinnerOfGift(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetWinnerOfGift), ClassName, new { GiftName = name });
            var winner = await _repository.GetWinnerOfGift(name);
            if (winner == null)
                LoggingHelper.LogNotFound(_logger, nameof(GetWinnerOfGift), ClassName, name);
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetWinnerOfGift), ClassName, new { WinnerName = winner.Name });
            return winner?.Name;
        }

        public async Task<ImageUploadResponseDTO> UploadGiftImageAsync(int giftId, IFormFile file)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(UploadGiftImageAsync), ClassName, new { GiftId = giftId, FileName = file.FileName });
            
            var gift = await _repository.GetGiftByIdAsync(giftId);
            if (gift == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(UploadGiftImageAsync), ClassName, giftId.ToString());
                throw new KeyNotFoundException($"Gift with id {giftId} not found.");
            }

            try
            {
                ValidateImageFile(file);

                var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "images");
                if (!Directory.Exists(assetsPath))
                {
                    Directory.CreateDirectory(assetsPath);
                }

                var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                var fileName = $"gift-{giftId}-{DateTime.Now.Ticks}.{fileExtension}";
                var filePath = Path.Combine(assetsPath, fileName);

                if (!string.IsNullOrEmpty(gift.ImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), gift.ImagePath.Replace("/", "\\"));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                gift.ImagePath = $"Assets/images/{fileName}";
                await _repository.UpdateGiftAsync(gift);

                LoggingHelper.LogCreated(_logger, nameof(UploadGiftImageAsync), ClassName, new { GiftId = giftId, ImagePath = gift.ImagePath });
                return new ImageUploadResponseDTO
                {
                    Success = true,
                    Message = "Image uploaded successfully.",
                    ImagePath = gift.ImagePath
                };
            }
            catch (ArgumentException ex)
            {
                LoggingHelper.LogValidationError(_logger, nameof(UploadGiftImageAsync), ClassName, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(UploadGiftImageAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<FileStream?> DownloadGiftImageAsync(int giftId)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(DownloadGiftImageAsync), ClassName, new { GiftId = giftId });
            
            var gift = await _repository.GetGiftByIdAsync(giftId);
            if (gift == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(DownloadGiftImageAsync), ClassName, giftId.ToString());
                throw new KeyNotFoundException($"Gift with id {giftId} not found.");
            }

            if (string.IsNullOrEmpty(gift.ImagePath))
            {
                LoggingHelper.LogNotFound(_logger, nameof(DownloadGiftImageAsync), ClassName, $"ImagePath for gift {giftId}");
                throw new KeyNotFoundException("Gift image path is not set.");
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), gift.ImagePath.Replace("/", "\\"));
            if (!File.Exists(filePath))
            {
                LoggingHelper.LogNotFound(_logger, nameof(DownloadGiftImageAsync), ClassName, filePath);
                throw new KeyNotFoundException("Gift image file not found on disk.");
            }

            LoggingHelper.LogMethodSuccess(_logger, nameof(DownloadGiftImageAsync), ClassName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        private void ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
                throw new ArgumentException("File size exceeds 5MB limit.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("File extension not allowed. Allowed extensions: jpg, jpeg, png, webp");
        }

        public async Task<FileStream?> GetGiftImageAsync(int giftId)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetGiftImageAsync), ClassName, new { GiftId = giftId });
            
            var gift = await _repository.GetGiftByIdAsync(giftId);
            if (gift == null || string.IsNullOrEmpty(gift.ImagePath))
            {
                LoggingHelper.LogNotFound(_logger, nameof(GetGiftImageAsync), ClassName, giftId.ToString());
                return null;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), gift.ImagePath.Replace("/", "\\"));
            if (!File.Exists(filePath))
            {
                LoggingHelper.LogNotFound(_logger, nameof(GetGiftImageAsync), ClassName, filePath);
                return null;
            }

            LoggingHelper.LogMethodSuccess(_logger, nameof(GetGiftImageAsync), ClassName);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}