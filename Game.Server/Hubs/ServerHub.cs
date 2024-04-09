using Game.Server.Interfaces;
using Game.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Game.Server.Hubs;

public class ServerHub : Hub
{
    private readonly ILogger<ServerHub> _logger;
    private readonly IDataContext _dataContext;
    private readonly Dictionary<int, GameInfo> _playingGames = new();
    private readonly Dictionary<int, List<Move>> _gamesMoves = new();

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
        
        _playingGames[titleGame] = new GameInfo(
            creator.UserName, creator.ConnectionId,
            secondPlayerName, Context.ConnectionId);
            
        await Clients.User(creator.ConnectionId).SendAsync("GameCreated", secondPlayerName);
        
        await Clients.AllExcept(creator.ConnectionId).SendAsync("RemoveOpenGame", titleGame);
        
        await Clients.Caller.SendAsync("GameEntered");

        _gamesMoves[titleGame] = new List<Move>();
        
        await Clients.User(NextMoveGenerate(_playingGames[titleGame])).SendAsync("NextMove");
    }

    private static string NextMoveGenerate(GameInfo gameInfo)
        => new Random().Next(1, 2) == 1 ? gameInfo.FirstPlayerConnectionId : gameInfo.SecondPlayerConnectionId;

    public async Task HandleMove(int gameTitle, string name, int x, int y, int z)
    {
        _logger.LogInformation("Handle move");

        var gameMoves = _gamesMoves[gameTitle];

        if (gameMoves.LastOrDefault()?.Name == name)
        {
            _logger.LogInformation("Игрок уже ходил на предыдущем ходу");

            await Clients.Caller.SendAsync("ErrorMessage", "Вы уже ходили на предыдущем ходу!");
            return;
        }

        if (IsMoveExist(gameMoves, x, y, z))
        {
            await Clients.Caller.SendAsync("ErrorMessage", "Данная клетка уже занята!");
            return;            
        }

        var move = new Move(name, x, y, z);

        gameMoves.Add(move);
    }

    private static bool IsMoveExist(IEnumerable<Move> moves, int x, int y, int z)
         => moves.Any(c => c.X == x && c.Y == y && c.Z == z);

    public async Task DisconnectWithError(string errorMessage)
    {
        _logger.LogError(errorMessage);
        await base.OnDisconnectedAsync(new HubException(errorMessage));
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogWarning("DicConnected");

        var gameTitle = _dataContext.DeleteInfoByConnectionId(Context.ConnectionId);

        _playingGames.Remove(gameTitle);

        _gamesMoves.Remove(gameTitle);
        
        await base.OnDisconnectedAsync(exception);
    }
}