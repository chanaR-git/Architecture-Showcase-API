
namespace Chinese_sale_api.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageAsync<T>(string key, T data);
    }
}