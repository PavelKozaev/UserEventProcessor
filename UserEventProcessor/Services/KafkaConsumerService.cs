using Confluent.Kafka;
using System.Text.Json;
using UserEventProcessor.Models;

namespace UserEventProcessor.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly EventObservable _eventObservable;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _topic;

        public KafkaConsumerService(EventObservable eventObservable)
        {
            _eventObservable = eventObservable;

            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
            _topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            var groupId = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID");

            if (string.IsNullOrEmpty(bootstrapServers) || string.IsNullOrEmpty(_topic) || string.IsNullOrEmpty(groupId))
            {
                throw new InvalidOperationException("Kafka environment variables (KAFKA_BOOTSTRAP_SERVERS, KAFKA_TOPIC, KAFKA_GROUP_ID) must be set.");
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[KafkaConsumer] Сервис запущен. Подписка на топик...");
            _consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        Console.WriteLine($"[KafkaConsumer] Получено сообщение из топика '{consumeResult.TopicPartitionOffset}': {consumeResult.Message.Value}");

                        var userEvent = JsonSerializer.Deserialize<UserEvent>(consumeResult.Message.Value);
                        if (userEvent != null)
                        {
                            _eventObservable.Publish(userEvent);

                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[KafkaConsumer] Ошибка при чтении из Kafka: {e.Error.Reason}");
                        Console.ResetColor();
                    }
                    catch (JsonException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[KafkaConsumer] Ошибка десериализации JSON: {e.Message}. Сообщение будет пропущено.");
                        Console.ResetColor();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[KafkaConsumer] Операция отменена. Завершение работы...");
            }
            finally
            {
                _consumer.Close();
            }
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();

            _eventObservable.Complete();
            Console.WriteLine("[KafkaConsumer] Ресурсы потребителя Kafka освобождены.");
        }
    }
}
