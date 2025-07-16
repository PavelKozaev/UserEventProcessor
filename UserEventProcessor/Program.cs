using UserEventProcessor.Data;
using UserEventProcessor.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        Console.WriteLine("Настройка сервисов...");

        services.AddSingleton<EventObservable>();


        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Переменная окружения POSTGRES_CONNECTION_STRING не установлена.");
        }

        services.AddNpgsqlDataSource(connectionString);

        services.AddSingleton<IDataStorage, PostgresDataStorage>();

        services.AddSingleton<StatisticsAggregatorObserver>();

        services.AddHostedService<KafkaConsumerService>();
    })
    .Build();

Console.WriteLine("Соединение Observable и Observer...");
var eventObservable = host.Services.GetRequiredService<EventObservable>();
var statsObserver = host.Services.GetRequiredService<StatisticsAggregatorObserver>();

IDisposable subscription = eventObservable.AsObservable().Subscribe(statsObserver);

Console.WriteLine("Запуск приложения...");

await host.RunAsync();

subscription.Dispose();
Console.WriteLine("Приложение остановлено.");