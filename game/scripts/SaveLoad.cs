using System;
using System.IO;
using System.Threading;

public class SaveLoad
{
    public static string Filename(int x)
    {
        return System.IO.Path.Combine(Godot.OS.GetUserDataDir(), $"file{x}");
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
        int which = Int32.Parse(reader9.ReadLine());
        reader9.Close();
        if (which == 1) {
            Console.WriteLine("Save file corrupted. Reverting to previous.");
        }
        StreamReader reader = new StreamReader(
            Filename(which)
        );
        GameState.Persistent.FromJSON(reader);
        reader.Close();
    }
    public static void Save()
    {
        Console.WriteLine("Save begin");
        GameState.Persistent.Sema.WaitOne();

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

        GameState.Persistent.Sema.Release();
        Console.WriteLine("Save end");
    }

    public static void SaveAsync() {
        Thread t = new Thread(new ThreadStart(Save));
        t.Start();
    }
}
