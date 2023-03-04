using Godot;
using System;
using System.Diagnostics;

public class SidePanel : PanelContainer
{
    public CircuitUI MyCircuitUI;
    public VBoxContainer VBox;
    public RichTextLabel ManaLabel;
    public RichTextLabel MoneyLabel;
    public WandSimulation MyWandSim;
    private float _manaFontHeight;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        ManaLabel = GetNode<RichTextLabel>("VBox/Crystal/Centerer/Mana");
        MoneyLabel = GetNode<RichTextLabel>("VBox/Money");
        VBox.Visible = false;
        MyWandSim = new WandSimulation(this);
        DynamicFont font = Shared.NewFont(60);
        ManaLabel.AddFontOverride("normal_font", font);
        _manaFontHeight = font.GetHeight();
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
            if (MyCircuitUI.MyCircuit == wand.MyCircuit)
                return;
            MyCircuitUI.QueueFree();
        }
        MyCircuitUI = new CircuitUI(wand, 0, false, false);
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
        ManaLabel.BbcodeText = $"[center]{MathBB.Build(GameState.Transient.Mana, _manaFontHeight)}[/center]";
        MoneyLabel.BbcodeText = $" [color=yellow]${MathBB.Build(GameState.Persistent.Money)}[/color]";
        Hold(GameState.Persistent.MyWand);
    }
}
