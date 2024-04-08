using Game.Server.Interfaces;
using Game.Server.Models;

namespace Game.Server.Data;

public class DataContext : IDataContext
{
    private readonly Dictionary<int, OpenGameInfo> openGames = new();

    public void AddNewValue(int key, OpenGameInfo openGameInfo)
    {
        openGames[key] = openGameInfo;
    }

    public void RemoveValue(int key)
    {
        openGames.Remove(key);
    }

    public OpenGameInfo GetValue(int key)
    {
        return openGames[key];
    }

    public ICollection<OpenGame> GetAllOpenGames()
    {
        return openGames.Select(c => new OpenGame(c.Key, c.Value.ConnectionId, c.Value.UserName)).ToList();
    }

    public ICollection<int> GetKeys()
    {
        return openGames.Keys;
    }
}