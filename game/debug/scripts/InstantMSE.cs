using Godot;
using System;

public class InstantMSE : Node2D
{
    private RichTextLabel _label;
    private CircuitUI _cui;
    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("Label");
        _cui = GetNode<CircuitUI>("CircuitUI");
        _cui.MyCircuit = GameState.Persistent.MyTypelessGem.MyCircuit;
        _cui.Rebuild();
        _cui.Connect(
            "modified", this, "circuitModified"
        );
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
