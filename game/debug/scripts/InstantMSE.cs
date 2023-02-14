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
        Simplest mse0 = CustomGem.MinimumSuperpositionEquilibrium(_cui.MyCircuit, 0);
        Console.WriteLine("mse1");
        Simplest mse1 = CustomGem.MinimumSuperpositionEquilibrium(_cui.MyCircuit, 1);
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
