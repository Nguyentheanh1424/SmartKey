using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Common.Interfaces.Services;

[ApiController]
[Route("api/debug")]
public class DebugSignalRController : ControllerBase
{
    private readonly IRealtimeService _realtime;

    public DebugSignalRController(IRealtimeService realtime)
    {
        _realtime = realtime;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send()
    {
        var userId = Guid.Parse("019b779f-4327-7a89-9346-198870196fe4");

        await _realtime.SendNotiToUserAsync(
            userId,
            "door.unlocked",
            new
            {
                type = "door.unlocked",
                payload = new { from = "API DEBUG" }
            }
        );

        return Ok("sent");
    }
}
