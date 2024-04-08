namespace Game.Server.Models;

public class GameInfo
{
    public GameInfo(string firstPlayerUserName, string secondPlayerUserName)
    {
        FirstPlayerUserName = firstPlayerUserName;
        SecondPlayerUserName = secondPlayerUserName;
    }

    public string FirstPlayerUserName { get; set; }
    public string SecondPlayerUserName { get; set; }

}