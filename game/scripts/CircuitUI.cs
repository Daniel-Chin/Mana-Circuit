using Godot;
using System;
using System.Collections.Generic;

public class CircuitUI : Node2D
{
    // code-defined
    [Signal] public delegate void modified();
    private static readonly int MAX_PARTICLES = 8;
    private static readonly double EMIT_LAMBDA = .5;
    private static readonly double ADVECT_LAMBDA = 8;
    // poisson distribution, prob / sec
    private static readonly float ADVECT_LEN = .3f;
    private static readonly float NOISE_MULT = 1.5f;
    private static readonly Vector2 HALF = new Vector2(.5f, .5f);
    private int _vh_sep;
    public Circuit MyCircuit;
    public int RecursionDepth;
    private ColorRect _bgRect;
    private GridContainer _grid;
    private GemListScene _GemList;
    private List<ParticleAndTrail> _pAndTs;
    private Queue<ParticleAndTrail> _pAndTsToFree;
    private PointInt _selectedLocation;
    public Color BackColor;
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
                Particle[] ps = Parent.MyCircuit.Advect(MyParticle, false, false);
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
    public CircuitUI() : base() { }
    public CircuitUI(Circuit circuit, int recursionDepth) : base()
    {
        MyCircuit = circuit;
        RecursionDepth = recursionDepth;
        Rebuild();
    }
    public override void _Ready()
    {
        _bgRect = GetNode<ColorRect>("Container/BG");
        _grid = GetNode<GridContainer>("Container/MyGrid");
        _GemList = GetNode<GemListScene>("MyGemList");
        _pAndTs = new List<ParticleAndTrail>();
        _pAndTsToFree = new Queue<ParticleAndTrail>();
        _vh_sep = _grid.GetConstant("vseparation");
        _GemList.Connect(
            "gemSelected", this, "onGemListGemSelect"
        );

        if (MyCircuit != null)
            Rebuild();
    }

    public void Rebuild()
    {
        Shared.QFreeChildren(_grid);
        _bgRect.Color = BackColor;
        _grid.Columns = MyCircuit.Size.IntX;
        for (int j = 0; j < MyCircuit.Size.IntY; j++)
        {
            for (int i = 0; i < MyCircuit.Size.IntX; i++)
            {
                Gem gem = MyCircuit.Field[i, j];
                GemUI gemUI = new GemUI(gem);
                _grid.AddChild(gemUI);
                gemUI.Button.Connect(
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
            List<Gem.Source> sources = MyCircuit.FindAll<Gem.Source>();
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
        _selectedLocation = new PointInt(i, j);
        switch (MyCircuit.Seek(_selectedLocation))
        {
            case Gem.Wall w:
            case Gem.Source s:
            case Gem.Drain d:
                return;
        }
        _GemList.ListAll();
        _GemList.MyDialog.PopupCentered();
    }

    public void onGemListGemSelect()
    {
        MyCircuit.Remove(_selectedLocation);
        Gem gem = _GemList.SelectedGem;
        if (gem != null)
        {
            gem.Location = _selectedLocation;
            MyCircuit.Add(gem);
            _GemList.SelectedGem = null;
        }
        Rebuild();
        EmitSignal("modified");
    }
}
