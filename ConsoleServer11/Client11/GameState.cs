public class GameState
{
    public int BlueX { get; set; } = 1;
    public int BlueY { get; set; } = 1;
    public int RedX { get; set; } = 18;
    public int RedY { get; set; } = 1;
    public int BlueScore { get; set; } = 0;
    public int RedScore { get; set; } = 0;
    public bool BlueDone { get; set; } = false;
    public bool RedDone { get; set; } = false;
    public bool GameOver { get; set; } = false;
    public string Result { get; set; } = "";

    public char[,] Map { get; set; } = GameMap.GetCopy();

    public string Serialize()
    {
        string mapStr = GameMap.Serialize(Map);
        return $"{BlueX} {BlueY} {RedX} {RedY} {BlueScore} {RedScore} {(GameOver ? 1 : 0)} {Result}\n{mapStr}";
    }

    public static GameState Deserialize(string data)
    {
        var lines = data.Split('\n', 2);
        var parts = lines[0].Split(' ');
        var state = new GameState
        {
            BlueX = int.Parse(parts[0]),
            BlueY = int.Parse(parts[1]),
            RedX = int.Parse(parts[2]),
            RedY = int.Parse(parts[3]),
            BlueScore = int.Parse(parts[4]),
            RedScore = int.Parse(parts[5]),
            GameOver = parts[6] == "1",
            Result = parts[7]
        };

        if (lines.Length > 1)
        {
            string mapStr = lines[1];
            for (int y = 0; y < GameMap.Height; y++)
                for (int x = 0; x < GameMap.Width; x++)
                    if (y * GameMap.Width + x < mapStr.Length)
                        state.Map[y, x] = mapStr[y * GameMap.Width + x];
        }

        return state;
    }
}