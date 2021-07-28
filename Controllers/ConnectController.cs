using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.Controllers
{
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private readonly ILogger<ConnectController> logger;

        public ConnectController(ILogger<ConnectController> logger)
        {
            this.logger = logger;
        }

        // [HttpGet]
        // [Route("/connect/")]
        // public async Task<IActionResult> Connect()
        // {
        //     if (!HttpContext.WebSockets.IsWebSocketRequest) return BadRequest();

        //     using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // }
    }
}
