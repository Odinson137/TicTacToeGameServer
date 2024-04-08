using Game.Server.Data;
using Game.Server.Hubs;
using Game.Server.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddSignalR();

services.AddSingleton<IDataContext, DataContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/GetAllOpenGames", async (
        ILogger<Program> logger, 
        IDataContext dataContext) =>
    {
        logger.LogInformation("Receive open games");
        var games = dataContext.GetAllOpenGames();
        logger.LogInformation("Received open games");
        return games;
    })
    .WithOpenApi();

app.MapHub<ServerHub>("/server");

app.MapGet("/", () => "This main page");

app.Run();

