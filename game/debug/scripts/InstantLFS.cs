using Godot;
using System;

public class InstantLFS : MarginContainer
{
    private RichTextLabel _label;
    private CircuitUI _cui;
    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("Label");
        _cui = new CircuitUI(
            GameState.Persistent.MyTypelessGem, 0
        );
        _cui.Connect(
            "modified", this, "circuitModified"
        );
        AddChild(_cui);
        MoveChild(_label, 2);
        circuitModified();
    }

    public void circuitModified()
    {
        // Console.WriteLine("mse0");
        // Console.WriteLine();
        // Simplest mse0 = _cui.MyCircuit.MinimumSuperpositionEquilibrium(0);
        // Console.WriteLine("mse1");
        // Simplest mse1 = _cui.MyCircuit.MinimumSuperpositionEquilibrium(1);
        // Console.WriteLine();
        // // Simplest first_order = Simplest.Eval(
        // //     mse1, Operator.MINUS, mse0
        // // );
        // // _label.Text = $"{first_order} * x + {mse0}";
        // _label.Text = $"{mse0}; {mse1}";
        GameState.Persistent.MyTypelessGem.Eval();
        Simplest s = GameState.Persistent.MyTypelessGem.CachedMultiplier;
        _label.BbcodeText = MathBB.Build(s);
    }

    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
