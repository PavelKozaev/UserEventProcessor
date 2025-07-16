using UserEventProcessor.Data;
using UserEventProcessor.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        Console.WriteLine("��������� ��������...");

        services.AddSingleton<EventObservable>();


        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("���������� ��������� POSTGRES_CONNECTION_STRING �� �����������.");
        }

        services.AddNpgsqlDataSource(connectionString);

        services.AddSingleton<IDataStorage, PostgresDataStorage>();

        services.AddSingleton<StatisticsAggregatorObserver>();

        services.AddHostedService<KafkaConsumerService>();
    })
    .Build();

Console.WriteLine("���������� Observable � Observer...");
var eventObservable = host.Services.GetRequiredService<EventObservable>();
var statsObserver = host.Services.GetRequiredService<StatisticsAggregatorObserver>();

IDisposable subscription = eventObservable.AsObservable().Subscribe(statsObserver);

Console.WriteLine("������ ����������...");

await host.RunAsync();

subscription.Dispose();
Console.WriteLine("���������� �����������.");