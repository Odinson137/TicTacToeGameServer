namespace Game.Server.Models;

public class Move
{
    public Move(string name, int x, int y, int z)
    {
        Name = name;
        X = x;
        Y = y;
        Z = z;
    }

    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
}