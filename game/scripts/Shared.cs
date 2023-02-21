using System;
using Godot;

public class Shared
{
    public static readonly bool DEBUG = true;

    public class ObjectStateIllegal : Exception { }
    public class ValueError : Exception { }
    public class TypeError : Exception { }
    public class PlayerCreatedEpsilonNaught : Exception { }
    public static Random Rand = new Random();
    public static readonly Theme THEME = GD.Load<Theme>("res://misc/theme.tres");
    public static readonly Font FONT = THEME.DefaultFont;
    public static readonly float FONT_SCALE = FONT.GetHeight() / 7f / 2f;
    public static readonly int MAX_RECURSION = 4;
    // 7f is the font's # of pixels in height

    public static void QFreeChildren(Node node)
    {
        foreach (Node x in node.GetChildren())
        {
            x.QueueFree();
        }
    }


    public static void PrintArray(object[] objs)
    {
        Console.Write("[");
        foreach (var x in objs)
        {
            Console.Write(x.ToString());
            Console.Write(", ");
        }
        Console.WriteLine("]");
    }
}

public abstract class MagicItem
{
    public abstract string Name();
    public abstract string DisplayName();
}

public interface Spawnable
{
    // NPC
    // First Wand
}
