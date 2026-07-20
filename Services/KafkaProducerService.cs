using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Chinese_sale_api.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string? _topicName;

        public KafkaProducerService(IConfiguration configuration)
        {
            // 1. שליפת הנתונים מה-appsettings
            var bootstrapServers = configuration["KafkaSettings:BootstrapServers"];
            _topicName = configuration["KafkaSettings:TopicName"];

            if(_topicName == null || bootstrapServers == null)
            {
                throw new InvalidOperationException("Missing or invalid TopicName or BootstrapServers in configuration.");
            }

            // 2. יצירת אובייקט ProducerConfig כפי שנדרש
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            // 3. שימוש במחלקה הרשמית ProducerBuilder
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendMessageAsync<T>(string key, T data)
        {
            var serializedData = JsonSerializer.Serialize(data);

            var message = new Message<string, string>
            {
                Key = key,
                Value = serializedData
            };

            // שליחת ההודעה ל-Kafka באופן אסינכרוני
            await _producer.ProduceAsync(_topicName, message);
        }
    }
}
