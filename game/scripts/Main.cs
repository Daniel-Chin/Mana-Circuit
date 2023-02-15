using Godot;
using System;
using System.Diagnostics;

public class Main : Node2D
{
    public static Main Singleton = null;
    public static float WorldTime;
    public override void _Ready()
    {
        Debug.Assert(Singleton == null);
        Singleton = this;
        WorldTime = 0f;
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
    }
}
