using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
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
            var donors = await _repository.GetDonorsAsync();
            if (donors == null)
            {
                _logger.LogWarning("donor repository returnrd no donors")
                return Enumerable.Empty<ReadDonorDTO>();
            }
            return donors.Select(d => ToReadDto(d));
        }

        public async Task<ReadDonorDTO?> GetDonorByIdAsync(int id)
        {
            var donor = await _repository.GetDonorByIdAsync(id);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByEmailAsync(string email)
        {
            
            var donor = await _repository.GetDonorByEmailAsync(email);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByNameAsync(string name)
        {
            var donor = await _repository.GetDonorByNameAsync(name);
            return donor is null ? null : ToReadDto(donor);
        }

        public async Task<ReadDonorDTO?> GetDonorByGiftAsync(int giftId)
        {
            var donor = await _repository.GetDonorByGiftAsync(giftId);
            return donor is null ? null : ToReadDto(donor);
        }

        //עשיתי שינויים פה וברפוזיטורי, לשאול את אביגיל
        public async Task<ReadDonorDTO?> AddDonorAsync(CreateDonorDTO dto)
        {
            var donor = new Donor
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                MyGifts = new List<Gift>()
            };

            var created = await _repository.AddDonorAsync(donor);
            return created is null ? null : ToReadDto(created);
        }

        public async Task<ReadDonorDTO?> UpdateDonorAsync(int id, UpdateDonorDTO dto)
        {
            var existing = await _repository.GetDonorByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            // preserve existing values for null fields in DTO
            var updatedEntity = new Donor
            {
                Name = dto.Name ?? existing.Name,
                Email = dto.Email ?? existing.Email,
                Phone = dto.Phone ?? existing.Phone
            };

            var updated = await _repository.UpdateDonorAsync(id, updatedEntity);
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
            var deleted = await _repository.DeleteDonorAsync(id);
            return deleted is null ? null : ToReadDto(deleted);
        }
    }
}