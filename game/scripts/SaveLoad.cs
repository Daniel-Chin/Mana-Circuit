using System;
using System.IO;

public class SaveLoad
{
    public static void Test()
    {
        GameState.PersistentClass p0 = GameState.Persistent;
        StreamWriter sw = new StreamWriter("C:/Users/iGlop/d/temp/1.json");
        GameState.Persistent.ToJSON(sw);
        sw.Flush();
        sw.Close();
        StreamReader sr = new StreamReader("C:/Users/iGlop/d/temp/1.json");
        GameState.Persistent = new GameState.PersistentClass();
        GameState.Persistent.FromJSON(sr);
        GameState.PersistentClass p1 = GameState.Persistent;
        Console.WriteLine(p0.HasGems);
        Console.WriteLine(p1.HasGems);
    }
}
