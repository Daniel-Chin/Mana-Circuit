using Godot;
using System;


public class Main : Node2D
{
    public static Main Singleton = null;
    public LowPanel MyLowPanel;
    public SidePanel MySidePanel;
    public World MyWorld;
    public MageUI MyMageUI;
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
        MyWorld = GetNode<World>("HBox/World");
        MyMageUI = GetNode<MageUI>("HBox/World/MageUI");
        MySidePanel = GetNode<SidePanel>("HBox/SidePanel");
        MyLowPanel = GetNode<LowPanel>("Overlay/VBox/LowPanel");
        MyWorld.Connect(
            "new_wand", this, "NewWand"
        );

        SaveLoad.Load();
        // GameState.Persistent.DebugInit();
        GameState.Transient.Update();
        NewWand();
        MyWorld.UpdateBack();
        Director.CheckEvent();
    }

    public override void _Process(float delta)
    {
        WorldTime += delta;
        Director.Process(delta);
    }

    public void NewWand()
    {
        MySidePanel.Hold(GameState.Persistent.MyWand);
        MyMageUI.Hold(GameState.Persistent.MyWand);
    }
}
