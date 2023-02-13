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
        // Test.Main();
        PackedScene pS = GD.Load<PackedScene>("res://GemUI.tscn");
        GemUI gemUI = pS.Instance<GemUI>();
        AddChild(gemUI);
        gemUI.Set(new Gem.Source(new PointInt(0, 1)));
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
    }
}
