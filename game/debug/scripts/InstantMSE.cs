using Godot;
using System;

public class InstantMSE : Node2D
{
    private Label _label;
    private CircuitUI _cui;
    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
        _cui = GetNode<CircuitUI>("CircuitUI");
        _cui.Connect(
            "modified", this, "circuitModified"
        );
        circuitModified();
    }

    public void circuitModified()
    {
        Console.WriteLine("mse0");
        Console.WriteLine();
        Simplest mse0 = _cui.MyCircuit.MinimumSuperpositionEquilibrium(0);
        Console.WriteLine("mse1");
        Simplest mse1 = _cui.MyCircuit.MinimumSuperpositionEquilibrium(1);
        Console.WriteLine();
        // Simplest first_order = Simplest.Eval(
        //     mse1, Operator.MINUS, mse0
        // );
        // _label.Text = $"{first_order} * x + {mse0}";
        _label.Text = $"{mse0}; {mse1}";
    }

    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
