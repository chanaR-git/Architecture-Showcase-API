using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using System;
using System.Threading;

namespace KafkaConsumer1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "logging-consumer-group", // מזהה קבוצת הצרכנים
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe("transaction-events");

            Console.WriteLine("Server [Consumer] is running and waiting for messages...");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    // תפיסת ההודעה שהגיעה מה-Broker
                    var result = consumer.Consume(cts.Token);

                    // הדפסה ללוג
                    Console.WriteLine($"\n[LOG] Received Event: {DateTime.Now}");
                    Console.WriteLine($"Key: {result.Message.Key}");
                    Console.WriteLine($"Body: {result.Message.Value}");
                }
            }
            catch (OperationCanceledException)
            {
                // סגירה נקייה של השרת
            }
            finally
            {
                consumer.Close();
            }

        }
    }
}
