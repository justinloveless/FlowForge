using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlowForge;

public class SchedulerHostedService: IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private List<Task> _tasks = new();
    private CancellationTokenSource _cts = new();

    public SchedulerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadExistingEventsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    public async Task AddEventAsync(ScheduleEvent scheduledEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISchedulingRepository>();
        await repository.AddScheduledEvent(scheduledEvent);

        ScheduleEvent(scheduledEvent);
    }
    
    private async Task LoadExistingEventsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISchedulingRepository>();
            var events = await repository.GetScheduledEvents();

            foreach (var evt in events)
            {
                ScheduleEvent(evt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading existing events: {ex.Message}");
        }
    }
    
    private void ScheduleEvent(ScheduleEvent scheduledEvent)
    {
        var task = Task.Run(async () =>
        {
            var delay = scheduledEvent.ResumeTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, _cts.Token);

            await ExecuteEventAsync(scheduledEvent);
        });

        _tasks.Add(task);
    }
    
    private async Task ExecuteEventAsync(ScheduleEvent scheduledEvent)
    {
        try
        {
            Console.WriteLine($"Executing event: {scheduledEvent.EventName}");
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISchedulingRepository>();
            var engine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();
            
            await engine.TriggerEventAsync(scheduledEvent.InstanceId, scheduledEvent.EventName, new Dictionary<string, object>(), "system");
            await repository.RemoveScheduledEvent(scheduledEvent);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing event {scheduledEvent.Id}: {ex.Message}");
        }
    }
}