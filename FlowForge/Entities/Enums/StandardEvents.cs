namespace FlowForge.Enums;

public enum StandardEvents
{
    WorkflowRegistered,
    WorkflowStarted,
    WorkflowStartedByEvent,
    StateTransitioned,
    StateProcessed,
    ConditionEvalFailure,
    ExceptionOccured,
    DependenciesSatisfied,
    UnauthorizedActorTriggeredEvent
}