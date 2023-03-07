using Godot;
using System;

public class SidePanel : PanelContainer
{
    public CircuitUI MyCircuitUI;
    public VBoxContainer VBox;
    public RichTextLabel ManaLabel;
    public RichTextLabel MoneyLabel;
    public RichTextLabel ManaRateLabel;
    public HBoxContainer JumperHBox;
    public WandSimulation MyWandSim;
    private float _manaFontHeight;
    private Simplest _manaAcc { get; set; }
    private float _timeAcc { get; set; }
    public override void _Ready()
    {
        _manaAcc = Simplest.Zero();
        _timeAcc = 0f;
        
        VBox = GetNode<VBoxContainer>("VBox");
        MoneyLabel = GetNode<RichTextLabel>("VBox/HBox/Money");
        ManaRateLabel = GetNode<RichTextLabel>("VBox/ManaRate");
        ManaLabel = GetNode<RichTextLabel>("VBox/Crystal/Centerer/Mana");
        JumperHBox = GetNode<HBoxContainer>("VBox/HBox/JumperHBox");
        VBox.Visible = false;
        MyWandSim = new WandSimulation(this);
        DynamicFont font = Shared.NewFont(60);
        ManaLabel.AddFontOverride("normal_font", font);
        _manaFontHeight = font.GetHeight();
        ManaRateLabel.Clear();
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
        _timeAcc += delta;
        if (_timeAcc >= 1) {
            _timeAcc --;
            SetManaRate(_manaAcc);
            _manaAcc = Simplest.Zero();
        }
    }

    public void ManaCrystalized(Simplest mana)
    {
        GameState.Transient.Mana = Simplest.Eval(
            GameState.Transient.Mana, Operator.PLUS, mana
        );
        _manaAcc = Simplest.Eval(
            _manaAcc, Operator.PLUS, mana
        );
        Update();
    }

    public new void Update()
    {
        ManaLabel.BbcodeText = $"[center][color=#00ffff]{MathBB.Build(GameState.Transient.Mana, _manaFontHeight)}[/color][/center]";
        MoneyLabel.BbcodeText = $" [color=yellow]${MathBB.Build(GameState.Persistent.Money)}[/color]";
        Hold(GameState.Persistent.MyWand);
        JumperHBox.Visible = Jumper.HasJumper();
    }

    public void SetManaRate(Simplest s) {
        ManaRateLabel.Clear();
        ManaRateLabel.PushAlign(RichTextLabel.Align.Center);
        ManaRateLabel.PushColor(Colors.Cyan);
        ManaRateLabel.AppendBbcode("+ ");
        ManaRateLabel.AppendBbcode(MathBB.Build(s));
        ManaRateLabel.AppendBbcode(" / sec");
        ManaRateLabel.Pop();
        ManaRateLabel.Pop();
    }
}
