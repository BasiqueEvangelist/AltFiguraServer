using System.Net.WebSockets;
using System.Threading.Tasks;
using AltFiguraServer.Data;
using AltFiguraServer.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AltFiguraServer.Controllers
{
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private readonly ILogger<WebSocketConnection> connLogger;
        private readonly FiguraState state;

        public ConnectController(ILogger<WebSocketConnection> connLogger, FiguraState state)
        {
            this.connLogger = connLogger;
            this.state = state;
        }

        [HttpGet]
        [Route("/connect/")]
        public async Task<IActionResult> Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return BadRequest();

            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            WebSocketConnection connection = new(webSocket, connLogger, state);
            await connection.Run();
            return new EmptyResult();
        }
    }
}
