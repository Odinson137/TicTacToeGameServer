namespace Game.Server.Models;

public class GameInfo
{
    public GameInfo(string firstPlayerUserName, string firstPlayerConnectionId, 
        string secondPlayerUserName, string secondPlayerConnectionId)
    {
        FirstPlayerUserName = firstPlayerUserName;
        SecondPlayerUserName = secondPlayerUserName;
        SecondPlayerConnectionId = secondPlayerConnectionId;
        FirstPlayerConnectionId = firstPlayerConnectionId;
    }

    public string FirstPlayerUserName { get; set; }
    public string FirstPlayerConnectionId { get; set; }
    public string SecondPlayerUserName { get; set; }
    public string SecondPlayerConnectionId { get; set; }

}