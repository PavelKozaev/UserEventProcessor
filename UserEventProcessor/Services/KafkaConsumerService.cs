using Confluent.Kafka;
using System.Text.Json;
using UserEventProcessor.Models;

namespace UserEventProcessor.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly EventObservable _eventObservable;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _topic;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, EventObservable eventObservable, IConfiguration configuration)
        {
            _logger = logger;
            _eventObservable = eventObservable;

            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            _topic = configuration["Kafka:Topic"];
            var groupId = configuration["Kafka:GroupId"];

            if (string.IsNullOrEmpty(bootstrapServers) || string.IsNullOrEmpty(_topic) || string.IsNullOrEmpty(groupId))
            {
                throw new InvalidOperationException("Настройки Kafka (Kafka:BootstrapServers, Kafka:Topic, Kafka:GroupId) должны быть установлены.");
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
            _logger.LogInformation("Сервис Kafka запущен. Подписка на топик '{Topic}'...", _topic);
            _consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Yield(); 

                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult is null) continue;

                        _logger.LogInformation("Получено сообщение из Kafka: Partition={Partition}, Offset={Offset}.",
                            consumeResult.TopicPartitionOffset.Partition, consumeResult.TopicPartitionOffset.Offset);

                        var userEvent = JsonSerializer.Deserialize<UserEvent>(consumeResult.Message.Value);
                        if (userEvent != null)
                        {
                            _eventObservable.Publish(userEvent);
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, "Ошибка при чтении из Kafka: {Reason}", e.Error.Reason);
                    }
                    catch (JsonException e)
                    {
                        _logger.LogWarning(e, "Ошибка десериализации JSON. Сообщение будет пропущено.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Операция отменена. Завершение работы Kafka consumer...");
            }
            finally
            {
                _consumer.Close();
            }
        }

        public override void Dispose()
        {
            _eventObservable.Complete();
            _consumer.Dispose();
            base.Dispose();
            _logger.LogInformation("Ресурсы потребителя Kafka освобождены.");
        }
    }
}
