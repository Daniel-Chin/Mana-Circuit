using System;
using System.IO;
using Godot;

public class SaveLoad
{   // todo: async
    public static string Filename(int x)
    {
        return System.IO.Path.Combine(OS.GetUserDataDir(), $"file{x}");
    }
    public static void Load()
    {
        StreamReader reader9;
        try
        {
            reader9 = new StreamReader(
                Filename(9)
            );
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("No save file found. New game!");
            Save();
            return;
        }
        reader9.Close();
        int which = Int32.Parse(reader9.ReadLine());
        StreamReader reader = new StreamReader(
            Filename(which)
        );
        GameState.Persistent.FromJSON(reader);
        reader.Close();
    }
    public static void Save()
    {
        StreamWriter writer1 = new StreamWriter(Filename(1));
        GameState.Persistent.ToJSON(writer1);
        writer1.Flush();
        writer1.Close();

        StreamWriter writer9;
        writer9 = new StreamWriter(Filename(9));
        writer9.WriteLine("1");
        writer9.Flush();
        writer9.Close();

        StreamWriter writer0 = new StreamWriter(Filename(0));
        GameState.Persistent.ToJSON(writer0);
        writer0.Flush();
        writer0.Close();

        writer9 = new StreamWriter(Filename(9));
        writer9.WriteLine("0");
        writer9.Flush();
        writer9.Close();
    }
}
