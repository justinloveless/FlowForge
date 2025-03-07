﻿namespace FlowForge;

public interface IDataProvider
{
    Task<object> GetDataAsync(string urlTemplate, WorkflowInstanceId instanceId, Dictionary<string, object> instanceData,
        Dictionary<string, object> stateData);
}