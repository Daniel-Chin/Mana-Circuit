using Godot;
using System;


public class Main : Node2D
{
    public static Main Singleton = null;
    public LowPanel MyLowPanel;
    public static float WorldTime;
    public Main() : base()
    {
        Shared.Assert(Singleton == null);
        Singleton = this;
    }
    public override void _Ready()
    {
        WorldTime = 0f;
        Director.MainUI = this;
        MyLowPanel = GetNode<LowPanel>("Overlay/VBox/LowPanel");

        // SaveLoad.Load();
        GameState.Persistent.DebugInit();
        Director.CheckEvent();
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
        Director.Process(delta);
    }
}
