using Npgsql;
using UserEventProcessor.Models;

namespace UserEventProcessor.Data
{
    public class PostgresDataStorage(NpgsqlDataSource dataSource) : IDataStorage
    {
        private readonly NpgsqlDataSource _dataSource = dataSource;

        public async Task SaveStatsAsync(IEnumerable<UserEventStat> stats, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[DataStorage] Сохранение {stats.Count()} записей в PostgreSQL...");

            const string commandText = @"
            INSERT INTO user_event_stats (user_id, event_type, count)
            VALUES ($1, $2, $3)
            ON CONFLICT (user_id, event_type)
            DO UPDATE SET count = EXCLUDED.count;";

            await using var batch = _dataSource.CreateBatch();

            foreach (var stat in stats)
            {
                var batchCommand = new NpgsqlBatchCommand(commandText)
                {
                    Parameters =
                    {
                        new NpgsqlParameter<long> { Value = stat.UserId },
                        new NpgsqlParameter<string> { Value = stat.EventType },
                        new NpgsqlParameter<int> { Value = stat.Count }
                    }
                };
                batch.BatchCommands.Add(batchCommand);
            }

            await batch.ExecuteNonQueryAsync(cancellationToken);

            Console.WriteLine("[DataStorage] Данные успешно сохранены в PostgreSQL.");
        }
    }
}
