using Chinese_sale_api.DTO;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Mvc;


namespace Chinese_sale_api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class DonorController : ControllerBase
    {
        public IDonorService _service;
        public DonorController(IDonorService srv)
        {
            _service = srv;
        }
        [HttpGet]
        public async Task<IActionResult> GetDonorsAsync()
        {
            var donors = await _service.GetDonorsAsync();
            return Ok(donors);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDonorByIdAsync([FromRoute] int id)
        {
            if (id == 0) return BadRequest("id is required");
            var donor = await _service.GetDonorByIdAsync(id);
            return Ok(donor);
        }
        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetDonorByNameAsync([FromRoute] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("name is required");
            var donor = await _service.GetDonorByNameAsync(name);
            return donor is null ? NotFound() : Ok(donor);
        }

        [HttpGet("byemail/{email}")]
        public async Task<IActionResult> GetDonorByEmailAsync([FromRoute] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("email is required");
            var donor = await _service.GetDonorByEmailAsync(email);
            return donor is null ?NotFound(): Ok(donor);
        }
        [HttpGet("bygift/{giftId}")]
        public async Task<IActionResult> GetDonorByGiftAsync([FromRoute] int giftId)
        {
            if (giftId==0) return BadRequest("gift id is required");
            var donor = await _service.GetDonorByGiftAsync(giftId);

            return donor is null ? NotFound() : Ok(donor);
        }
        [HttpPost]
        public async Task<IActionResult> AddDonorAsync([FromBody] CreateDonorDTO newDonor)
        {
            try
            {
                var addDonor = await _service.AddDonorAsync(newDonor);
                return Ok(addDonor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonorAsync([FromRoute] int id)
        {
            var deletedDonor = _service.DeleteDonorAsync(id);
            return deletedDonor is null ? NotFound() : Ok(deletedDonor);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDonorAsync([FromRoute] int id, [FromBody] UpdateDonorDTO toUpdate)
        {
            var updatedDonor = _service.UpdateDonorAsync(id, toUpdate);
            return updatedDonor is null ? NotFound() : Ok(updatedDonor);
        }
    }
}

