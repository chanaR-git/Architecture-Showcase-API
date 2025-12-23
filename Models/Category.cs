namespace Chinese_sale_api.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Gift>? Gifts { get; set; }
    }
}
