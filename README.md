# FlowForge

**FlowForge** is a dynamic and extensible workflow engine designed to 
simplify and automate complex processes. Built with flexibility and 
scalability in mind, FlowForge allows developers to define, manage, 
and execute workflows seamlessly. Whether you're orchestrating business 
logic, automating processes, or handling asynchronous events, 
FlowForge provides the tools you need to forge workflows into efficient, 
reliable systems.

---

## FlowForge Project structure
Each of the folders in the root of the directory are different projects or
modules that can be added to the core FlowForge library to extend it's functionality.

### `/FlowForge` *(the core library)*
This is where you will find the core FlowForge logic. There is also a README in there 
that explains more about how to get started with FlowForge.
### `/FlowForge.Postgresql`
This is a module for supporting persisting the FlowForge data within Postgres. 
### `/FlowForge.RabbitMQ`
This is a module for supporting using RabbitMQ as an event queue.
### `/FlowForge.Mermaid`
This is a module that adds the ability to generate mermaid diagrams based on the 
workflow definition. 
### `/FlowForge.Demo`
This is a demo project that shows some ways that you use FlowForge. It includes an API
and a UI. 
### `/Tests`
This is where all the unit and integration tests live. 


--- 

## Contributing
We welcome contributions to FlowForge! If you'd like to contribute, please follow these steps:
1. Fork the repository.
2. Create a feature branch: `git checkout -b feature-name`.
3. Commit your changes: `git commit -m 'Add feature'`.
4. Push to the branch: `git push origin feature-name`.
5. Submit a pull request.

---

## Adding a new module
If you want to add support for another database or event queue, or if you have an idea of 
a feature that you'd like to add to FlowForge, but it doesn't make sense to include
in the core library, then adding a module is the way to go!

Adding a module requires you to create a new project that depends on the core FlowForge
package. Then, feel free to add any extension methods that you need. Check out the 
following projects for examples of how to extend FlowForge:
- `FlowForge.Postgres`:
    
    This extends the `WorkflowEngineBuilder` to inject custom `IWorkflowRepository` and `IEventRepository` implementations.

- `FlowForge.RabbitMQ`:

  This also extends the `WorkflowEngineBuilder` and injects a custom `IWorkflowEventPublisher` implementation. It also adds a hosted message consumer service that runs independently.
 
- `FlowForge.Mermaid`:
  
  This extends the `FlowForge` facade directly, enabling it to add completely new functionality to the core library. 

--- 

## License
FlowForge is licensed under the MIT License.

---

## Feedback and Support
If you encounter issues or have feature requests, please open an issue in the Github repository. For questions or feedback, feel free to contact me directly at `justinlovelessx@gmail.com`.