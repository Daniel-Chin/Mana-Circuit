using Godot;
using System;
using System.Diagnostics;

public class Main : Node2D
{
    public static Main Singleton = null;
    public static float WorldTime;
    public Main() : base()
    {
        Debug.Assert(Singleton == null);
        Singleton = this;
        GameState.Persistent.Init();
        GameState.Transient.Init();
    }
    public override void _Ready()
    {
        WorldTime = 0f;
        // Test.Main();
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
    }
}
