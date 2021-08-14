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
        private readonly Database db;

        public ConnectController(ILogger<WebSocketConnection> connLogger, Database db)
        {
            this.connLogger = connLogger;
            this.db = db;
        }

        [HttpGet]
        [Route("/connect/")]
        public async Task<IActionResult> Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return BadRequest();

            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            WebSocketConnection connection = new(webSocket, connLogger, new FiguraState(db));
            await connection.Run();
            return new EmptyResult();
        }
    }
}
