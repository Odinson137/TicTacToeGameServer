using System.Collections;
using Game.Server.Interfaces;
using Game.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Game.Server.Hubs;

public class ServerHub : Hub
{
    private readonly ILogger<ServerHub> _logger;
    private readonly IDataContext _dataContext;
    private readonly Dictionary<int, GameInfo> _playingGames;
    private readonly Dictionary<int, List<Move>> _gamesMoves;

    public ServerHub(ILogger<ServerHub> logger, IDataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
        _playingGames = dataContext.PlayingGames;
        _gamesMoves = dataContext.GamesMoves;
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
    
    public async Task<int> CreateGame(string firstPlayerName)
    {
        _logger.LogInformation("Create game");
        var gameTitle = GenerateTitleGame();

        _dataContext.AddNewValue(gameTitle, new OpenGameInfo(Context.ConnectionId, firstPlayerName));

        await Clients.Caller.SendAsync("GetGameTitle", gameTitle);
        
        await Clients.AllExcept(Context.ConnectionId).SendAsync("ShowNewGame", gameTitle, firstPlayerName);
        
        return gameTitle;
    }
    
    public async Task<ICollection<OpenGame>> GetOpenGames()
    {
        _logger.LogInformation("Receive open games");
        
        var games = _dataContext.GetAllOpenGames();
        
        _logger.LogInformation("Received open games");
        return games;
    }

    public async Task EnterGame(int titleGame, string secondPlayerName)
    {
        _logger.LogInformation("Enter game");
        var creator = _dataContext.GetValue(titleGame);
        _dataContext.RemoveValue(titleGame);
        
        _playingGames[titleGame] = new GameInfo(
            creator.UserName, creator.ConnectionId,
            secondPlayerName, Context.ConnectionId);
            
        _logger.LogInformation($"{titleGame}");
        _logger.LogInformation($"{creator.UserName} - {creator.ConnectionId}");
        _logger.LogInformation($"{secondPlayerName} - {Context.ConnectionId}");
        
        _gamesMoves.Add(titleGame, new List<Move>());
        
        await Clients.Client(creator.ConnectionId).SendAsync("GameCreated", secondPlayerName);
        
        await Clients.AllExcept(creator.ConnectionId).SendAsync("RemoveOpenGame", titleGame);
        
        await Clients.Caller.SendAsync("GameEntered");
        
        var generate = NextMoveGenerate(_playingGames[titleGame]);
        _logger.LogInformation($"Рандомный id - {generate}");
        await Clients.Client(generate).SendAsync("NextMove");
    }

    private static string NextMoveGenerate(GameInfo gameInfo)
        => new Random().Next(1, 2) == 1 ? gameInfo.FirstPlayerConnectionId : gameInfo.SecondPlayerConnectionId;

    public async Task HandleMove(int gameTitle, string name, int x, int y, int z)
    {
        _logger.LogInformation("Handle move");
        _logger.LogInformation(gameTitle.ToString());
        _logger.LogInformation(name);
        _logger.LogInformation($"{x} - {y} - {z}");

        foreach (var keyValuePair in _gamesMoves)
        {
            _logger.LogInformation($"{keyValuePair.Key} - {keyValuePair.Value}");
        }
        
        var gameMoves = _gamesMoves[gameTitle];

        // if (gameMoves.LastOrDefault()?.Name == name)
        // {
        //     _logger.LogInformation("Игрок уже ходил на предыдущем ходу");
        //
        //     await Clients.Caller.SendAsync("ErrorMessage", "Вы уже ходили на предыдущем ходу!");
        //     return;
        // }

        if (IsMoveExist(gameMoves, x, y, z))
        {
            await Clients.Caller.SendAsync("ErrorMessage", "Данная клетка уже занята!");
            return;            
        }

        var move = new Move(name, x, y, z);

        gameMoves.Add(move);

        var game = _playingGames[gameTitle];
        if (game.FirstPlayerConnectionId == Context.ConnectionId)
        {
            await Clients.Client(game.SecondPlayerConnectionId).SendAsync("OpponentMoveHandler", x, y, z);
        }
        else
        {
            await Clients.Client(game.FirstPlayerConnectionId).SendAsync("OpponentMoveHandler", x, y, z);
        }
    }

    private static bool IsMoveExist(IEnumerable<Move> moves, int x, int y, int z)
         => moves.Any(c => c.X == x && c.Y == y && c.Z == z);

    public async Task DisconnectGame(int gameTitle)
    {
        if (_playingGames[gameTitle].FirstPlayerConnectionId == Context.ConnectionId)
        {
            await Clients.Client(_playingGames[gameTitle].SecondPlayerConnectionId).SendAsync("OpponentLiveGame");
        }
        else
        {
            await Clients.Client(_playingGames[gameTitle].FirstPlayerConnectionId).SendAsync("OpponentLiveGame");
        }
        
        _playingGames.Remove(gameTitle);
        _gamesMoves.Remove(gameTitle);
    }

    public async Task DisconnectWithError(string errorMessage)
    {
        _logger.LogError(errorMessage);
        await base.OnDisconnectedAsync(new HubException(errorMessage));
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogWarning("DicConnected");

        _dataContext.DeleteInfoByConnectionId(Context.ConnectionId);
        
        var game = 
            _playingGames.FirstOrDefault(c => 
                c.Value.FirstPlayerConnectionId == Context.ConnectionId
                || c.Value.SecondPlayerConnectionId == Context.ConnectionId);
        
        _playingGames.Remove(game.Key);
        
        _gamesMoves.Remove(game.Key);
        
        await base.OnDisconnectedAsync(exception);
    }
}