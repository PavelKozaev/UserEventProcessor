using UserEventProcessor.Models;

namespace UserEventProcessor.Data
{
    public interface IDataStorage
    {
        Task SaveStatsAsync(IEnumerable<UserEventStat> stats, CancellationToken cancellationToken = default);
    }
}
