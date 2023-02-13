using Godot;
using System;

public class Main : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Test.Main();
        PackedScene pS = GD.Load<PackedScene>("res://GemUI.tscn");
        GemUI gemUI = pS.Instance<GemUI>();
        AddChild(gemUI);
        gemUI.Set(new Gem.Source(new PointInt(0, 1)));
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
