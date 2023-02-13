using Godot;
using System;

public class CircuitUI : Node2D
{
    private Circuit _circuit;
    private GridContainer _grid;
    private PackedScene _gemUIPrefab;
    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("MyGrid");
        _gemUIPrefab = GD.Load<PackedScene>("res://GemUI.tscn");

        Circuit c = new Circuit(new PointInt(8, 8));
        for (int i = 0; i < 8; i++)
        {
            c.Add(new Gem.Wall().Place(new PointInt(0, i)), true);
            c.Add(new Gem.Wall().Place(new PointInt(7, i)), true);
            c.Add(new Gem.Wall().Place(new PointInt(i, 0)), true);
            c.Add(new Gem.Wall().Place(new PointInt(i, 7)), true);
        }

        Gem source = new Gem.Source(new PointInt(1, 0)).Place(new PointInt(1, 4));
        c.Add(source);

        Gem drain = new Gem.Drain().Place(new PointInt(6, 4));
        c.Add(drain);

        c.Add(new Gem.WeakMult().Place(new PointInt(2, 2)));
        c.Add(new Gem.Mirror(false).Place(new PointInt(3, 1)));
        c.Add(new Gem.Mirror(true).Place(new PointInt(4, 1)));
        c.Add(new Gem.Stochastic(false).Place(new PointInt(4, 4)));
        c.Add(new Gem.Focus(new PointInt(1, 0)).Place(new PointInt(3, 4)));
        _circuit = c;

        Rebuild();
    }

    private void Rebuild()
    {
        foreach (Node x in _grid.GetChildren())
        {
            x.QueueFree();
        }
        _grid.Columns = _circuit.Size.IntX;
        for (int j = 0; j < _circuit.Size.IntY; j++)
        {
            for (int i = 0; i < _circuit.Size.IntX; i++)
            {
                GemUI gemUI = _gemUIPrefab.Instance<GemUI>();
                _grid.AddChild(gemUI);
                Gem gem = _circuit.Field[i, j];
                gemUI.Set(gem);
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
