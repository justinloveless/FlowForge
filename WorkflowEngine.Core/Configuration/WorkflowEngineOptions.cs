﻿namespace WorkflowEngine.Core;

public class WorkflowEngineOptions
{
    public bool UseInMemoryRepositories { get; set; } = true;
    public bool EnableDetailedLogging { get; set; } = false;
}