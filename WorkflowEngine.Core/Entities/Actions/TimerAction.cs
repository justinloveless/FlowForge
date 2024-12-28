using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.Core;

public class TimerAction : IWorkflowAction
{
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
        // Simulate timer
        _ = Task.Run(async () =>
        {
            var localServices = serviceProvider.CreateScope().ServiceProvider;
            var delay = resumeTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }

            var engine = localServices.GetRequiredService<IWorkflowEngine>();
            await engine.TriggerEventAsync(instance.Id, "Resume", new Dictionary<string, object>(), "system");
        });
        
    }
}