using Microsoft.AspNetCore.Mvc;

namespace FlowForge.Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class DataProviderController : ControllerBase
{
    
    [HttpGet("userstate/{userId}")]
    public IActionResult GetUserState(string userId)
    {
        Console.WriteLine($"GetUserState: {userId}");
        return Ok(
            """
            {
            "state": "Texas"
            }
            """
        );
    }
    [HttpGet("userage/{userId}")]
    public IActionResult GetUserAge(string userId)
    {
        Console.WriteLine($"GetUserAge: {userId}");
        return Ok(
            """
            {
            "age": 19
            }
            """
        );
    }
    
    [HttpGet("trainings/{instanceId:guid}")]
    public IActionResult GetTrainings(Guid instanceId)
    {
        Console.WriteLine($"GetTrainings: {instanceId}");
        var result = new
        {
            Classes = new List<Training>()
            {
                new() {
                    Name = "How to get gud",
                    Cost = 420,
                    CompletedOn = DateTime.Now,
                },
                
                new() {
                    Name = "How to get gud",
                    Cost = 420,
                    CompletedOn = DateTime.Now,
                }
            },
            UserId = instanceId.ToString(),
            UserName = "John Doe",
        };
        
        return Ok(result);
    }

    public class Training
    {
        public string Name { get; set; }
        public double Cost { get; set; }
        public DateTime CompletedOn { get; set; }
    }

}