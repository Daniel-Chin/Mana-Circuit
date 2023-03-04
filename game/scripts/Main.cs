using Godot;
using System;


public class Main : Node2D
{
    public static Main Singleton = null;
    public VBoxContainer VBoxLowPanel;
    public LowPanel MyLowPanel;
    public SidePanel MySidePanel;
    public World MyWorld;
    public MageUI MyMageUI;
    public Revive MyRevive;
    public WindowDialog MadeEpsilon0;
    public static float MainTime;
    public Main() : base()
    {
        Shared.Assert(Singleton == null);
        Singleton = this;
    }
    public override void _Ready()
    {
        MainTime = 0f;
        Director.MainUI = this;
        MyWorld = GetNode<World>("HBox/World");
        MyMageUI = GetNode<MageUI>("HBox/World/MageUI");
        VBoxLowPanel = GetNode<VBoxContainer>("Overlay/VBoxLowPanel");
        MySidePanel = GetNode<SidePanel>("HBox/SidePanel");
        MyLowPanel = GetNode<LowPanel>("Overlay/VBoxLowPanel/LowPanel");
        MyRevive = GetNode<Revive>("Overlay/VBox2/HBox/Revive");
        MadeEpsilon0 = GetNode<WindowDialog>("MadeEpsilon0");
        MyWorld.Connect(
            "wand_replaced", this, "WandReplaced"
        );
        MyWorld.Connect(
            "player_died", this, "PlayerDied"
        );
        VBoxLowPanel.Visible = false;

        SaveLoad.Load();
        GameState.Transient.Update();
        WandReplaced();
        MyWorld.UpdateBack();
        MySidePanel.Update();
        Director.CheckEvent();
    }

    public override void _Process(float delta)
    {
        MainTime += delta;
        Director.Process(delta);
        Jumper.Process(delta);
    }

    public void WandReplaced()
    {
        MySidePanel.Update();
        MyMageUI.Hold(GameState.Persistent.MyWand);
    }

    public void PlayerDied()
    {
        MyRevive.Activate();
    }
}
