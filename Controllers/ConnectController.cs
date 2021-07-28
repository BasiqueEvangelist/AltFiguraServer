using System.Net.WebSockets;
using System.Threading.Tasks;
using AltFiguraServer.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.Controllers
{
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private readonly ILogger<WebSocketConnection> connLogger;

        public ConnectController(ILogger<WebSocketConnection> connLogger)
        {
            this.connLogger = connLogger;
        }

        [HttpGet]
        [Route("/connect/")]
        public async Task<IActionResult> Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return BadRequest();

            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            WebSocketConnection connection = new(webSocket, connLogger);
            await connection.Run();
            return new EmptyResult();
        }
    }
}
