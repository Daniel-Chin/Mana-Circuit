using Godot;
using System;
using System.Diagnostics;

public class Main : Node2D
{
    public static Main Singleton = null;
    public LowPanel MyLowPanel;
    public static float WorldTime;
    public Main() : base()
    {
        Debug.Assert(Singleton == null);
        Singleton = this;
    }
    public override void _Ready()
    {
        WorldTime = 0f;
        Director.MainUI = this;
        MyLowPanel = GetNode<LowPanel>("Overlay/VBox/LowPanel");
        Director.CheckEvent();
        SaveLoad.Test();
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
        Director.Process(delta);
    }
}
