using Godot;
using System;

public class SidePanel : PanelContainer
{
    public CircuitUI MyCircuitUI;
    public VBoxContainer VBox;
    public RichTextLabel ManaLabel;
    public RichTextLabel MoneyLabel;
    public WandSimulation MyWandSim;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        ManaLabel = GetNode<RichTextLabel>("VBox/Crystal/Centerer/Mana");
        MoneyLabel = GetNode<RichTextLabel>("VBox/Money");
        VBox.Visible = false;
        MyWandSim = new WandSimulation(this);
        Update();
    }

    public void Hold(Wand wand)
    {
        MyWandSim.MyWand = wand;
        if (wand == null)
        {
            VBox.Visible = false;
            return;
        }
        if (MyCircuitUI != null)
        {
            MyCircuitUI.QueueFree();
        }
        MyCircuitUI = new CircuitUI(wand, 0, true);
        VBox.AddChild(MyCircuitUI);
        VBox.Visible = true;
    }

    public override void _Process(float delta)
    {
        if (GameState.Transient.WorldPaused)
            return;
        MyWandSim.Process(delta);
    }

    public void ManaCrystalized(Simplest mana)
    {
        GameState.Transient.Mana = Simplest.Eval(
            GameState.Transient.Mana, Operator.PLUS,
            mana
        );
        Update();
    }

    public new void Update()
    {
        ManaLabel.BbcodeText = $"[center]{MathBB.Build(GameState.Transient.Mana)}[/center]";
        MoneyLabel.BbcodeText = $" [color=yellow]${MathBB.Build(GameState.Persistent.Money)}[/color]";
        Hold(GameState.Persistent.MyWand);
    }
}
