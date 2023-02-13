using Godot;
using System;

public class CircuitUI : Node2D
{
    Circuit MyCircuit;
    public override void _Ready()
    {
        Circuit c = new Circuit(new PointInt(8, 8));
        for (int i = 0; i < 8; i++)
        {
            c.Add(new Gem.Wall().Place(new PointInt(0, i)), true);
            c.Add(new Gem.Wall().Place(new PointInt(7, i)), true);
            c.Add(new Gem.Wall().Place(new PointInt(i, 0)), true);
            c.Add(new Gem.Wall().Place(new PointInt(i, 7)), true);
        }

        Gem source = new Gem.Source(new PointInt(1, 0)).Place(new PointInt(1, 3));
        c.Add(source);

        Gem drain = new Gem.Drain().Place(new PointInt(6, 3));
        c.Add(drain);

        c.Add(new Gem.Doubler().Place(new PointInt(2, 1)));
        c.Add(new Gem.Mirror(false).Place(new PointInt(3, 0)));
        c.Add(new Gem.Mirror(true).Place(new PointInt(4, 0)));
        c.Add(new Gem.Stochastic(false).Place(new PointInt(4, 3)));
        c.Add(new Gem.Focus(new PointInt(1, 0)).Place(new PointInt(3, 3)));
        MyCircuit = c;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
