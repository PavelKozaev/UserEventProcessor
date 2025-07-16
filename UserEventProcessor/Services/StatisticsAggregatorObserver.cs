using System.Collections.Concurrent;
using UserEventProcessor.Data;
using UserEventProcessor.Models;

namespace UserEventProcessor.Services
{
    public class StatisticsAggregatorObserver : IObserver<UserEvent>
    {
        private readonly IDataStorage _dataStorage;

        private readonly ConcurrentDictionary<(long userId, string eventType), int> _stats = new();

        public StatisticsAggregatorObserver(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
            Console.WriteLine("[Observer] Агрегатор статистики готов к работе.");
        }

        public void OnNext(UserEvent userEvent)
        {
            var key = (userEvent.UserId, userEvent.EventType);

            _stats.AddOrUpdate(key, 1, (existingKey, existingValue) => existingValue + 1);

            Console.WriteLine($"[Observer] Обработано событие: UserId={userEvent.UserId}, Type={userEvent.EventType}. Новый счетчик: {_stats[key]}");
        }

        public void OnError(Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Observer] В потоке событий произошла ошибка: {error.Message}");
            Console.ResetColor();
        }

        public void OnCompleted()
        {
            Console.WriteLine("[Observer] Поток событий завершен. Сохранение итоговой статистики...");

            var finalStats = _stats.Select(pair => new UserEventStat
            {
                UserId = pair.Key.userId,
                EventType = pair.Key.eventType,
                Count = pair.Value
            }).ToList();

            if (finalStats.Any())
            {
                _dataStorage.SaveStatsAsync(finalStats).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("[Observer] Нет данных для сохранения.");
            }
        }
    }
}
