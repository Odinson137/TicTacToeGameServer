using Game.Server.Data;
using Game.Server.Interfaces;
using Game.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Game.Server.Hubs;

public class ServerHub : Hub
{
    private readonly ILogger<ServerHub> _logger;
    private readonly IDataContext _dataContext;
    private readonly Dictionary<int, GameInfo> playingGames = new();

    public ServerHub(ILogger<ServerHub> logger, IDataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Подключился новый пользователь!");
        await base.OnConnectedAsync();
    }

    private int GenerateTitleGame()
    {
        Random rnd = new Random();
        var randomNumber = rnd.Next(100, 1000);

        while (_dataContext.GetKeys().Contains(randomNumber))
        {
            randomNumber = rnd.Next(100, 1000);
        }

        return randomNumber;
    }
    
    public async Task CreateGame(string firstPlayerName)
    {
        _logger.LogInformation("Create game");
        var gameTitle = GenerateTitleGame();

        _dataContext.AddNewValue(gameTitle, new OpenGameInfo(Context.ConnectionId, firstPlayerName));

        await Clients.Caller.SendAsync("GetGameTitle", gameTitle);
        await Clients.AllExcept(Context.ConnectionId).SendAsync("ShowNewGame", gameTitle, firstPlayerName);
    }

    public async Task EnterGame(int titleGame, string secondPlayerName)
    {
        _logger.LogInformation("Enter game");
        var creator = _dataContext.GetValue(titleGame);
        _dataContext.RemoveValue(titleGame);
        
        playingGames[titleGame] = new GameInfo(creator.UserName, secondPlayerName);
            
        await Clients.User(creator.ConnectionId).SendAsync("GameCreated", secondPlayerName);
        
        await Clients.Caller.SendAsync("GameEntered");
    }

    public async Task HandleMove(int x, int y, int z)
    {
        _logger.LogInformation("Handle move");
        // TODO обработать порядок ходов
    }

    private async Task DisconnectWithError(string errorMessage)
    {
        _logger.LogError(errorMessage);
        await base.OnDisconnectedAsync(new HubException(errorMessage));
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // var connectorJson = await _redis.StringGetAsync(GetConnectionId());
        // if (!string.IsNullOrEmpty(connectorJson))
        // {
        //     var connector = JsonConvert.DeserializeObject<ConnecterDTO>(connectorJson!);
        //     await _redis.KeyDeleteAsync(GetConnectionId());
        //     var count = await _redis.StringDecrementAsync($"forumCount:{connector!.BookId}");
        //     await Clients.All.SendAsync("UserCountMessage", count);
        //     await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group:{connector.BookId}");
        // }

        _logger.LogWarning("DicConnected");

        await base.OnDisconnectedAsync(exception);
    }
}