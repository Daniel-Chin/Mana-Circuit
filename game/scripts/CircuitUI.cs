using Godot;
using System;
using System.Collections.Generic;

public class CircuitUI : Node2D
{
    private static readonly int MAX_PARTICLES = 8;
    private static readonly double EMIT_LAMBDA = .5;
    private static readonly double ADVECT_LAMBDA = 8;
    // poisson distribution, prob / sec
    private static readonly float ADVECT_LEN = .3f;
    private static readonly float NOISE_MULT = 1.5f;
    private static readonly Vector2 HALF = new Vector2(.5f, .5f);
    private int _vh_sep;
    private Circuit _circuit;
    private GridContainer _grid;
    private PackedScene _gemUIPrefab;
    private PocketScene _pocket;
    private List<ParticleAndTrail> _pAndTs;
    private Queue<ParticleAndTrail> _pAndTsToFree;
    private class ParticleAndTrail
    {
        public CircuitUI Parent;
        public Particle MyParticle;
        public ManaTrail MyTrail;
        public Vector2 Follower;
        public ParticleAndTrail(CircuitUI parent, Gem.Source s)
        {
            Parent = parent;
            MyParticle = new Particle(
                s.Location, s.Direction, Simplest.Ones(1)
            );
            Follower = s.Location.ToVector2();
            MyTrail = new ManaTrail();
            parent.AddChild(MyTrail);
        }
        public void Follow()
        {
            // if (!MyTrail.IsReady) return;
            Vector2 d = MyParticle.Location.ToVector2() - Follower;
            if (d.Length() < ADVECT_LEN)
            {
                Particle[] ps = Parent._circuit.Advect(MyParticle, false, false);
                if (ps.Length == 0)
                {
                    // wall or drain
                    Parent._pAndTsToFree.Enqueue(this);
                    // Console.Write("Drain got mana ");
                    // Console.WriteLine(MyParticle.Mana[0]);
                }
                else
                {
                    MyParticle = ps[0];
                    Follow();
                }
                return;
            }
            d = d.Normalized();
            d.x += ((float)Shared.Rand.NextDouble() - .5f) * NOISE_MULT;
            d.y += ((float)Shared.Rand.NextDouble() - .5f) * NOISE_MULT;
            d *= ADVECT_LEN;
            Follower += d;
            MyTrail.ArriveAt(Parent.ToUICoords(Follower));
        }
        public void Free()
        {
            MyTrail.QueueFree();
        }
    }
    public override void _Ready()
    {
        _gemUIPrefab = GD.Load<PackedScene>("res://GemUI.tscn");
        _grid = GetNode<GridContainer>("MyGrid");
        _pocket = GetNode<PocketScene>("MyPocket");
        _pAndTs = new List<ParticleAndTrail>();
        _pAndTsToFree = new Queue<ParticleAndTrail>();
        _vh_sep = _grid.GetConstant("vseparation");

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

        c.Add(new Gem.WeakMult().Place(new PointInt(3, 2)));
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
                gemUI.Connect(
                    "pressed", this, "OnClickGem",
                    new Godot.Collections.Array() { i, j }
                );
            }
        }
    }

    public override void _Process(float delta)
    {
        foreach (ParticleAndTrail pT in _pAndTs)
        {
            if (Shared.Rand.NextDouble() < ADVECT_LAMBDA * delta)
                pT.Follow();
        }
        while (_pAndTsToFree.Count != 0)
        {
            ParticleAndTrail pT = _pAndTsToFree.Dequeue();
            pT.Free();
            _pAndTs.Remove(pT);
        }
        if (
            _pAndTs.Count < MAX_PARTICLES
            && Shared.Rand.NextDouble() < EMIT_LAMBDA * delta
        )
        {
            // spawn new particle
            // Console.WriteLine("spawn new particle");
            List<Gem.Source> sources = _circuit.FindAll<Gem.Source>();
            Gem.Source s = sources[Shared.Rand.Next(sources.Count)];
            ParticleAndTrail pT = new ParticleAndTrail(this, s);
            _pAndTs.Add(pT);
        }
    }

    private Vector2 ToUICoords(Vector2 circuitCoords)
    {
        int unit = (int)(_grid.GetChildren()[0] as GemUI).RectSize.x + _vh_sep;
        return (circuitCoords + HALF) * unit;
    }

    public void OnClickGem(int i, int j)
    {
        _pocket.MyDialog.Popup_();
    }
}
