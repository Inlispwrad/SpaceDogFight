using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using SpaceDogFight.Server.Game.Core;
using SpaceDogFight.Server.Game.Net;
using SpaceDogFight.Server.Game.Room;
using SpaceDogFight.Shared.Protocols;

var builder = WebApplication.CreateBuilder(args);

// Heart Beats 20s
builder.Services.AddWebSockets(opts => opts.KeepAliveInterval = TimeSpan.FromSeconds(20));

// Dependencies Injection
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<RoomManagerHandler>();
builder.Services.AddSingleton<RoomHandler>();
builder.Services.AddSingleton<MsgDispatcher>();



var app = builder.Build();
app.UseWebSockets();

var dispatcher = app.Services.GetRequiredService<MsgDispatcher>();
var connectionManager = app.Services.GetRequiredService<ConnectionManager>();
connectionManager.SetDispatcher(dispatcher);

app.Map("/ws", async _context =>
{
    if (!_context.WebSockets.IsWebSocketRequest)
    {
        _context.Response.StatusCode = 400;
        return;
    }

    var socket = await _context.WebSockets.AcceptWebSocketAsync();
    var playerId = _context.Request.Query["pid"].ToString();
    if(string.IsNullOrWhiteSpace(playerId)) 
        playerId = Guid.NewGuid().ToString("N");
    
    var connectMgr = _context.RequestServices.GetRequiredService<ConnectionManager>();
    await connectMgr.HandleAsync(playerId, socket);
});

// Launch App (Default Listen http://0.0.0.0:5000)
app.Run("http://0.0.0.0:5000");
