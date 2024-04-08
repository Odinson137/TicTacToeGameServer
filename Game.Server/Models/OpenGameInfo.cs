namespace Game.Server.Models;

public class OpenGameInfo
{
    public OpenGameInfo(string connectionId, string userName)
    {
        ConnectionId = connectionId;
        UserName = userName;
    }

    public string ConnectionId { get; set; }
    public string UserName { get; set; }
}