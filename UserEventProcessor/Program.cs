using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reactive.Linq;
using UserEventProcessor.Data;
using UserEventProcessor.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<EventObservable>();

        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("������ ����������� 'Postgres' �� ������� � ������������.");
        }
        services.AddNpgsqlDataSource(connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDataStorage, PostgresDataStorage>();
        services.AddScoped<StatisticsAggregator>();
        services.AddHostedService<KafkaConsumerService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("�������� � ���������� �������� ���� ������...");
try
{
    using var scope = host.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();

    logger.LogInformation("�������� ������� ���������.");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "��������� ������ �� ����� ���������� �������� ��.");
    return;
}

logger.LogInformation("��������� ��������� ��������� �������...");

var eventObservable = host.Services.GetRequiredService<EventObservable>();

IDisposable subscription = eventObservable.AsObservable()
    .Buffer(TimeSpan.FromSeconds(30), 500)
    .Where(batch => batch.Any())
    .Select(batch => Observable.FromAsync(async (cancellationToken) =>
    {
        await using var scope = host.Services.CreateAsyncScope();
        var aggregator = scope.ServiceProvider.GetRequiredService<StatisticsAggregator>();

        try
        {
            await aggregator.ProcessAndSaveBatchAsync(batch, cancellationToken);
        }
        catch (Exception ex)
        {
            var scopeLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            scopeLogger.LogError(ex, "��������� �������������� ������ ��� ��������� ����� �������.");
        }
    }))
    .Concat()
    .Subscribe(
        _ => { },
        ex => logger.LogCritical(ex, "� ������� ��������� ��������� ��������� ����������� ������!"),
        () => logger.LogInformation("������� �������� ��������� ��������.")
    );


logger.LogInformation("������ ����������...");

await host.RunAsync();

subscription.Dispose();
logger.LogInformation("���������� �����������.");