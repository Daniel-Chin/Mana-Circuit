using System;
using Godot;

public class Shared
{
    public class ObjectStateIllegal : Exception { }
    public class ValueError : Exception { }
    public class PlayerCreatedEpsilonNaught : Exception { }
    public static Random Rand = new Random();

    public static void QFreeChildren(Node node)
    {
        foreach (Node x in node.GetChildren())
        {
            x.QueueFree();
        }
    }
}
