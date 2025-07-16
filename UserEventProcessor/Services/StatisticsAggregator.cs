using UserEventProcessor.Data;
using UserEventProcessor.Models;

namespace UserEventProcessor.Services
{
    public class StatisticsAggregator
    {
        private readonly IDataStorage _dataStorage;
        private readonly ILogger<StatisticsAggregator> _logger;

        public StatisticsAggregator(IDataStorage dataStorage, ILogger<StatisticsAggregator> logger)
        {
            _dataStorage = dataStorage;
            _logger = logger;
            _logger.LogInformation("Агрегатор статистики создан.");
        }
        public async Task ProcessAndSaveBatchAsync(IList<UserEvent> events, CancellationToken cancellationToken)
        {
            if (events == null || !events.Any())
            {
                _logger.LogInformation("Получена пустая пачка событий, сохранение не требуется.");
                return;
            }

            _logger.LogInformation("Начало обработки пачки из {EventCount} событий.", events.Count);

            var batchStats = new Dictionary<(long userId, string eventType), int>();

            foreach (var userEvent in events)
            {
                var key = (userEvent.UserId, userEvent.EventType);
                batchStats.TryGetValue(key, out var currentCount);
                batchStats[key] = currentCount + 1;
            }

            var statsToSave = batchStats.Select(pair => new UserEventStat
            {
                UserId = pair.Key.userId,
                EventType = pair.Key.eventType,
                Count = pair.Value
            }).ToList();

            if (statsToSave.Any())
            {
                _logger.LogInformation("Агрегировано {StatsCount} уникальных записей статистики. Сохранение...", statsToSave.Count);
                await _dataStorage.SaveStatsAsync(statsToSave, cancellationToken);
            }
            else
            {
                _logger.LogInformation("В пачке не было данных для формирования статистики.");
            }
        }
    }
}
