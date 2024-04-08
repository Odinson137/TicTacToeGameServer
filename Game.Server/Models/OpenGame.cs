namespace Game.Server.Models;

public class OpenGame
{
    public OpenGame(int gameTitle, string connectionId, string userName)
    {
        ConnectionId = connectionId;
        UserName = userName;
        GameTitle = gameTitle;
    }

    public int GameTitle { get; set; }
    public string ConnectionId { get; set; }
    public string UserName { get; set; }
}