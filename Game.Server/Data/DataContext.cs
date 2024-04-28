using Game.Server.Interfaces;
using Game.Server.Models;

namespace Game.Server.Data;

public class DataContext : IDataContext
{
    private readonly Dictionary<int, OpenGameInfo> _openGames = new();
    public Dictionary<int, GameInfo> PlayingGames { get; } = new();
    public Dictionary<int, List<Move>> GamesMoves { get; } = new();

    public void AddNewValue(int key, OpenGameInfo openGameInfo)
    {
        _openGames[key] = openGameInfo;
    }

    public void RemoveValue(int key)
    {
        _openGames.Remove(key);
    }

    public OpenGameInfo GetValue(int key)
    {
        return _openGames[key];
    }

    public ICollection<OpenGame> GetAllOpenGames()
    {
        return _openGames.Select(c => new OpenGame(c.Key, c.Value.ConnectionId, c.Value.UserName)).ToList();
    }

    public ICollection<int> GetKeys()
    {
        return _openGames.Keys;
    }

    public int DeleteInfoByConnectionId(string connectionId)
    {
        var game = 
            _openGames.FirstOrDefault(c => c.Value.ConnectionId == connectionId);

        _openGames.Remove(game.Key);

        return game.Key;
    }
}