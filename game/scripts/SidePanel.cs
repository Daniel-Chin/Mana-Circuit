using Godot;
using System;

public class SidePanel : PanelContainer
{
    public CircuitUI MyCircuitUI;
    public VBoxContainer VBox;
    public RichTextLabel label;
    public WandSimulation MyWandSim;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        label = GetNode<RichTextLabel>("VBox/Crystal/Centerer/Label");
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
        if (GameState.Transient.NPCPausedWorld)
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
        label.BbcodeText = $"[center]{MathBB.Build(GameState.Transient.Mana)}[/center]";
    }
}
