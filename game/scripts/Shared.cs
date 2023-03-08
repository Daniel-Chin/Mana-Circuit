using System;
using System.Collections.Generic;
using Godot;

public class Shared
{
    public static readonly bool DEBUG_MATH = false;

    public class ObjectStateIllegal : Exception { }
    public class ValueError : Exception { }
    public class TypeError : Exception { }
    public class PlayerCreatedEpsilonNaught : Exception { }
    public class AssertionFailed : Exception { }
    public class SupposedlyUnreachable : Exception { }
    public static Random Rand = new Random();
    public static readonly Theme THEME = GD.Load<Theme>("res://misc/theme.tres");
    public static readonly Font FONT = THEME.DefaultFont;
    public static readonly DynamicFontData FONT_DATA = GD.Load<DynamicFontData>(
        "res://misc/font/m5x7.ttf"
    );
    public static readonly int MAX_RECURSION = 4;
    public static readonly Vector2 RESOLUTION = new Vector2(
        (int)ProjectSettings.GetSetting("display/window/size/width"),
        (int)ProjectSettings.GetSetting("display/window/size/height")
    );

    public static void QFreeChildren(Node node)
    {
        foreach (Node x in node.GetChildren())
        {
            x.QueueFree();
        }
    }

    public static void QFreeList<T>(List<T> list)
    {
        foreach (T x in list)
        {
            if (x is Node n)
            {
                n.QueueFree();
            }
            else
            {
                throw new Shared.TypeError();
            }
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
    public static void PrintList<T>(List<T> l)
    {
        Console.Write("List[");
        foreach (T x in l)
        {
            Console.Write(x.ToString());
            Console.Write(", ");
        }
        Console.WriteLine("]");
    }

    public static void Assert(bool x)
    {
        if (!x)
            throw new AssertionFailed();
    }

    public static DynamicFont NewFont(int fontSize)
    {
        DynamicFont font = new DynamicFont();
        font.FontData = GD.Load<DynamicFontData>("res://misc/font/m5x7.ttf");
        font.Size = fontSize;
        return font;
    }

    public static void PropogateMouseFilter(Control c, Control.MouseFilterEnum e) {
        c.MouseFilter = e;
        foreach (Node n in c.GetChildren()) {
            PropogateMouseFilter(n as Control, e);
        }
    }

    private static Vector2? CachedDPI = null;
    public static Vector2 DPI() {
        if (CachedDPI is Vector2 result) {
            return result;
        }
        CachedDPI = (
            Main.Singleton.GetViewport().GetTexture().GetData().GetSize()
            / RESOLUTION
        );
        return DPI();
    }
}

public abstract class MagicItem
{
    public abstract string Name();
    public abstract string DisplayName();
}
