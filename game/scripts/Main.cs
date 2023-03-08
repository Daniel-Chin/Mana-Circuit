using Godot;
using System;

public class Main : Node2D
{
    public static Main Singleton = null;
    public VBoxContainer VBoxLowPanel;
    public LowPanel MyLowPanel;
    public SidePanel MySidePanel;
    public World MyWorld;
    public Revive MyRevive;
    public PauseScreen MyPauseScreen;
    public WindowDialog MadeEpsilon0;
    public static float MainTime;
    public Image Screenshot;
    private float _shotAcc;
    public Main() : base()
    {
        Shared.Assert(Singleton == null);
        Singleton = this;
        _shotAcc = 0;
    }
    public override void _Ready()
    {
        MainTime = 0f;
        Director.MainUI = this;
        MyWorld = GetNode<World>("HBox/World");
        VBoxLowPanel = GetNode<VBoxContainer>("Overlay/VBoxLowPanel");
        MySidePanel = GetNode<SidePanel>("HBox/SidePanel");
        MyLowPanel = GetNode<LowPanel>("Overlay/VBoxLowPanel/LowPanel");
        MyRevive = GetNode<Revive>("Overlay/VBox2/HBox/Revive");
        MyPauseScreen = GetNode<PauseScreen>("PauseScreen");
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
        _shotAcc -= delta;
        if (_shotAcc < 0) {
            _shotAcc += Params.SEC_PER_SCREENSHOT;
            Screenshot = GetViewport().GetTexture().GetData();
            Screenshot.Resize(
                (int)Shared.RESOLUTION.x, (int)Shared.RESOLUTION.y, 
                Image.Interpolation.Nearest
            );
        }
        Director.Process(delta);
        Jumper.Process(delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause")) {
            if (Director.NowEvent == null) {
                if (MyPauseScreen.Visible) {
                    MyPauseScreen.Resume();
                } else if (!GameState.Transient.WorldPaused) {
                    Director.PauseWorld();
                    MyPauseScreen.PopupCentered();
                }
            }
        } else if (
            @event is InputEventMouseButton emb
            && emb.ButtonIndex == (int)ButtonList.Left
            && emb.Pressed
        ) {
            if (VBoxLowPanel.Visible) {
                MyLowPanel.Clicked();
            } else {
                if (!GameState.Transient.WorldPaused)
                    MyWorld.DoAttack();
            }
        }
    }

    public void WandReplaced()
    {
        MySidePanel.Update();
        MyWorld.MyMageUI.Hold(GameState.Persistent.MyWand);
    }

    public void PlayerDied()
    {
        MyRevive.Activate();
    }

    public Image ScreenshotRect(Control control) {
        float flippedY = (
            Shared.RESOLUTION.y - control.RectGlobalPosition.y
            - control.RectSize.y
        );
        Image img = Screenshot.GetRect(new Rect2(
            new Vector2(
                control.RectGlobalPosition.x, flippedY
            ), 
            control.RectSize
        ));
        img.FlipY();
        return img;
    }
}
