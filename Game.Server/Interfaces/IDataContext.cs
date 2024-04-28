using Game.Server.Models;

namespace Game.Server.Interfaces;

public interface IDataContext
{
    public Dictionary<int, GameInfo> PlayingGames { get; }
    public Dictionary<int, List<Move>> GamesMoves { get; }
    public void AddNewValue(int key, OpenGameInfo openGameInfo);
    public void RemoveValue(int key);
    public OpenGameInfo GetValue(int key);
    public int DeleteInfoByConnectionId(string connectionId);
    public ICollection<OpenGame> GetAllOpenGames();
    public ICollection<int> GetKeys();
}