# FlowForge

**FlowForge** is a dynamic and extensible workflow engine designed to simplify and automate complex processes. Built with flexibility and scalability in mind, FlowForge allows developers to define, manage, and execute workflows seamlessly. Whether you're orchestrating business logic, automating processes, or handling asynchronous events, FlowForge provides the tools you need to forge workflows into efficient, reliable systems.

---

## Features

- **Dynamic Workflow Definitions**  
  Define workflows using JSON, YAML, or a fluent API builder, giving you flexibility in how you model your processes.

- **Parallel and Sequential Execution**  
  Supports sequential steps, parallel execution, and synchronization of tasks.

- **Event-Driven Architecture**  
  Trigger workflows and transitions based on custom events, ensuring responsiveness to external inputs.

- **OnEnter and OnExit Actions**  
  Execute customizable actions on entering or exiting a state, including:
    - Webhook calls
    - Timer-based delays
    - Custom behaviors

- **Pluggable Components**  
  Integrates easily with your infrastructure:
    - **Databases**: Supports PostgreSQL, MongoDB, SQL Server, etc.
    - **Queues**: RabbitMQ, Kafka, or custom implementations.

- **Built-in Scheduler**  
  Add delays or schedule transitions at specific times.

- **State Management**  
  Track workflow progress and transitions with detailed event logs for auditing and debugging.

- **Assignment Rules**  
  Assign users or groups to states, ensuring only authorized actors can trigger transitions.

- **Extensible Action System**  
  Register custom actions for bespoke behavior, allowing seamless integration with your existing systems.

- **Visualization Tools**  
  Generate Mermaid diagrams directly from workflow definitions for easy visualization and debugging.

---

## Installation

Install the core FlowForge package via NuGet:

```bash
dotnet add package FlowForge
```
To integrate specific modules (e.g. PostgreSQL or RabbitMQ):
```bash
dotnet add package FlowForge.PostgreSQL
dotnet add package FlowForge.RabbitMQ
```

---

## Getting Started

### 1. Define a Workflow
You can define workflows using JSON or the provided fluent builder. 

**Using the Fluent Builder**:
```csharp
var workflow = new WorkflowDefinitionBuilder("Sample Workflow")
    .Start(s => s
        .OnEnter(new WebhookAction("http://example.com/start"))
    )
    .Delay(TimeSpan.FromMinutes(5))
    .ActionableStep("Approval", s => s
        .AssignGroup("managers")
        .OnEnter(new WebhookAction("http://example.com/approval"))
        .Transition("event == \"Approved\"")
        .Transition("event == \"Declined\"", "Start")
    )
    .End();
```

### 2. Register the Workflow
```csharp
await workflowEngine.RegisterWorkflowAsync(workflow);
```

---

### 3. Start a Workflow instance
```csharp
var instanceId = await workflowEngine.StartWorkflowAsync(workflowId, new Dictionary<string, object>
{
    { "initialData", "example" }
});
```

--- 

### 4. Trigger Events
```csharp
await workflowEngine.TriggerEventAsync(instanceId, "Approved", new Dictionary<string, object>
{
    { "reviewer", "admin" }
});
```

---

### 5. Visualize the Workflow
Use the **Mermaid Diagram Generator** to create visual representations of your workflows:
```csharp
var diagram = workflowEngine.GenerateMermaidDiagram(workflowDefinition);
Console.WriteLine(diagram);
```

--- 

### Modules
FlowForge is modula, allowing you to select the components that best fit your infrastructure:
#### Database Modules
- **PostgreSQL:** `FlowForge.PostgreSQL`
- **MongoDB:** `FlowForge.MongoDB` (coming soon)
#### Event Queue Modules
- **RabbitMQ**: `FlowForge.RabbitMQ`
- **Kafka:** `FlowForge.Kafka` (coming soon)

--- 

## Example Configuration
In your `Program.cs`:
```csharp
builder.Services.AddWorkflowEngine(options =>
{
    options.EnableDetailedLogging = true;
})
.UsePostgresql("Host=localhost;Database=workflow;Username=postgres;Password=password")
.UseRabbitMQ("localhost", "workflow-queue");
```

--- 

## Why Choose FlowForge?
1. **Flexibility**: Support for a variety of databases, event queues, and custom actions.
2. **Extensibility**: Easily add new modules or integrations.
3. **User-Friendly**: Fluent APIs, YAML support, and visualization tools make workflows easy to define and manage.
4. **Performance**: Designed to handle high-throughput and complex workflows with ease.

--- 

## Contributing
We welcome contributions to FlowForge! If you'd like to contribute, please follow these steps:
1. Fork the repository.
2. Create a feature branch: `git checkout -b feature-name`.
3. Commit your changes: `git commit -m 'Add feature'`.
4. Push to the branch: `git push origin feature-name`.
5. Submit a pull request.

---

## License
FlowForge is licensed under the MIT License. 

---

## Feedback and Support
If you encounter issues or have feature requests, please open an issue in the Github repository. For questions or feedback, feel free to contact me directly at `justinlovelessx@gmail.com`.