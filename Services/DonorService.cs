using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chinese_sale_api.Services
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _repository;
        private readonly ILogger<DonorService> _logger;
        private const string ClassName = nameof(DonorService);

        public DonorService(IDonorRepository repo, ILogger<DonorService> logger)
        {
            _repository = repo;
            _logger = logger;
        }
        private static ReadDonorDTO ToReadDto(Donor d) =>
            new ReadDonorDTO
            {
                Id = d.Id,
                Name = d.Name,
                Email = d.Email,
                Phone = d.Phone
            };

        public async Task<IEnumerable<ReadDonorDTO>> GetDonorsAsync()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetDonorsAsync), ClassName);
            var donors = await _repository.GetDonorsAsync();
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetDonorsAsync), ClassName, donors.Count());
            return donors.Select(d => ToReadDto(d));
        }

        public async Task<ReadDonorDTO?> GetDonorByIdAsync(int id)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByIdAsync), ClassName, new { DonorId = id });
            var donor = await _repository.GetDonorByIdAsync(id);
            if (donor is null)
                LoggingHelper.LogNotFound(_logger, nameof(GetDonorByIdAsync), ClassName, id.ToString());
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByIdAsync), ClassName);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByEmailAsync(string email)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByEmailAsync), ClassName, new { DonorEmail = email });
            var donor = await _repository.GetDonorByEmailAsync(email);
            if (donor is null)
                LoggingHelper.LogNotFound(_logger, nameof(GetDonorByEmailAsync), ClassName, email);
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByEmailAsync), ClassName);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByNameAsync(string name)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByNameAsync), ClassName, new { DonorName = name });
            var donor = await _repository.GetDonorByNameAsync(name);
            if (donor is null)
                LoggingHelper.LogNotFound(_logger, nameof(GetDonorByNameAsync), ClassName, name);
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByNameAsync), ClassName);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByGiftAsync(int giftId)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByGiftAsync), ClassName, new { GiftId = giftId });
            var donor = await _repository.GetDonorByGiftAsync(giftId);
            if (donor is null)
                LoggingHelper.LogNotFound(_logger, nameof(GetDonorByGiftAsync), ClassName, giftId.ToString());
            else
                LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByGiftAsync), ClassName);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> AddDonorAsync(CreateDonorDTO dto)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(AddDonorAsync), ClassName, new { dto.Email, dto.Name });
            var existing = await _repository.GetDonorByEmailAsync(dto.Email);
            if (existing != null)
            {
                LoggingHelper.LogDuplicateAttempt(_logger, nameof(AddDonorAsync), ClassName, $"Email: {dto.Email}");
                throw new InvalidOperationException("Email already exists");
            }

            var donor = new Donor
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                MyGifts = new List<Gift>()
            };

            var created = await _repository.AddDonorAsync(donor);
            LoggingHelper.LogCreated(_logger, nameof(AddDonorAsync), ClassName, new { created.Id, created.Email });
            return ToReadDto(created);
        }

        public async Task<ReadDonorDTO?> UpdateDonorAsync(int id, UpdateDonorDTO dto)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(UpdateDonorAsync), ClassName, new { DonorId = id });
            var existing = await _repository.GetDonorByIdAsync(id);
            if (existing == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(UpdateDonorAsync), ClassName, id.ToString());
                return null;
            }
            if (!string.IsNullOrWhiteSpace(dto.Email)) 
            {
                var existingEmailDonor = await _repository.GetDonorByEmailAsync(dto.Email);
                if (existingEmailDonor != null && existingEmailDonor.Id != id)
                {
                    LoggingHelper.LogDuplicateAttempt(_logger, nameof(UpdateDonorAsync), ClassName, $"Email: {dto.Email}");
                    throw new InvalidOperationException("Email already exists");
                }
            }
            existing.Name = dto.Name ?? existing.Name;
            existing.Email = dto.Email ?? existing.Email;
            existing.Phone = dto.Phone ?? existing.Phone;

            var updated = await _repository.UpdateDonorAsync(existing);
            LoggingHelper.LogUpdated(_logger, nameof(UpdateDonorAsync), ClassName, new { updated.Id, updated.Email });

            return updated is null ? null : new ReadDonorDTO
            {
                Id = updated.Id,
                Name = updated.Name,
                Email = updated.Email,
                Phone = updated.Phone
            };
        }

        public async Task<ReadDonorDTO?> DeleteDonorAsync(int id)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(DeleteDonorAsync), ClassName, new { DonorId = id });
            var deleted = await _repository.DeleteDonorAsync(id);
            if (deleted is null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(DeleteDonorAsync), ClassName, id.ToString());
                return null;
            }
            LoggingHelper.LogDeleted(_logger, nameof(DeleteDonorAsync), ClassName, id.ToString());
            return ToReadDto(deleted);
        }
    }
}