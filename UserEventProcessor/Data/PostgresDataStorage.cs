﻿using Npgsql;
using UserEventProcessor.Models;

namespace UserEventProcessor.Data
{
    public class PostgresDataStorage : IDataStorage
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<PostgresDataStorage> _logger;

        public PostgresDataStorage(NpgsqlDataSource dataSource, ILogger<PostgresDataStorage> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task SaveStatsAsync(IEnumerable<UserEventStat> stats, CancellationToken cancellationToken = default)
        {
            var statsList = stats.ToList();
            _logger.LogInformation("[DataStorage] Сохранение {Count} записей в PostgreSQL...", statsList.Count);

            const string commandText = @"
            INSERT INTO user_event_stats (user_id, event_type, count)
            VALUES ($1, $2, $3)
            ON CONFLICT (user_id, event_type)
            DO UPDATE SET count = user_event_stats.count + EXCLUDED.count;";

            await using var batch = _dataSource.CreateBatch();

            foreach (var stat in statsList)
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

            var affectedRows = await batch.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("[DataStorage] Данные успешно сохранены. Затронуто строк: {AffectedRows}.", affectedRows);
        }
    }
}
