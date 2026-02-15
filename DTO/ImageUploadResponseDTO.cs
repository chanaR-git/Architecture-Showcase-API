namespace Chinese_sale_api.DTO
{
    public class ImageUploadResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
