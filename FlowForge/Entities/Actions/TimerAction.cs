using Microsoft.Extensions.DependencyInjection;

namespace FlowForge;

public class TimerAction : WorkflowAction, IWorkflowAction
{
    private readonly IServiceProvider _provider;

    public TimerAction(IServiceProvider provider)
    {
        _provider = provider;
        Type = "Timer";
        Parameters = new Dictionary<string, object>();
    }

    public TimerAction(TimeSpan relativeDelay)
    {
        Type = "Timer";
        Parameters = new Dictionary<string, object> { { "relativeDelay", relativeDelay } };
    }

    public TimerAction(DateTime absoluteSchedule)
    {
        Type = "Timer";
        Parameters = new Dictionary<string, object> { { "absoluteSchedule", absoluteSchedule } };
    }
    private const string _type = "Timer";
    public async Task ExecuteAsync(WorkflowInstance instance, IDictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var hasDelay = parameters.TryGetValue("relativeDelay", out var delay);
        var hasSchedule = parameters.TryGetValue("absoluteSchedule", out var schedule);
                        
                        
        DateTime now = DateTime.Now;
        DateTime? triggerTime = null;

        if (hasSchedule)
        {
            triggerTime = DateTime.Parse(schedule.ToString());
        } else if (hasDelay)
        {
            triggerTime = now + TimeSpan.Parse(delay.ToString());
        }

        var resumeTime = triggerTime.Value;
        var localServices = serviceProvider.CreateScope().ServiceProvider;
        var _schedulerHostedService = localServices.GetRequiredService<SchedulerHostedService>();
        await _schedulerHostedService.AddEventAsync(new ScheduleEvent
        {
            EventName = "Resume", 
            InstanceId = instance.Id, 
            ResumeTime = resumeTime,
        });
        // Simulate timer
        // _ = Task.Run(async () =>
        // {
        //     var localServices = serviceProvider.CreateScope().ServiceProvider;
        //     var delay = resumeTime - DateTime.UtcNow;
        //     if (delay > TimeSpan.Zero)
        //     {
        //         await Task.Delay(delay);
        //     }
        //
        //     var engine = localServices.GetRequiredService<IWorkflowEngine>();
        //     await engine.TriggerEventAsync(instance.Id, "Resume", new Dictionary<string, object>(), "system");
        // });
        
    }
}