using Godot;
using System;

using System.Collections.Generic;

public class CircuitUI : AspectRatioContainer
{
    // code-defined
    [Signal] public delegate void new_explain();
    [Signal] public delegate void modified();
    private static readonly int MAX_PARTICLES = 8;
    private static readonly double EMIT_LAMBDA = .5;
    private static readonly double ADVECT_LAMBDA = 8;
    // poisson distribution, prob / sec
    private static readonly float ADVECT_LEN = .3f;
    private static readonly float NOISE_MULT = 1.5f;
    private static readonly Vector2 HALF = new Vector2(.5f, .5f);
    private static Shader _rainbow = GD.Load<Shader>("res://Rainbow.gdshader");
    public Circuit MyCircuit;
    public int RecursionDepth;
    private MarginContainer _container;
    private Control _bgRect;
    private GridContainer _grid;
    private GemListScene _GemList;
    private List<ParticleAndTrail> _pAndTs;
    private Queue<ParticleAndTrail> _pAndTsToFree;
    private PointInt _selectedLocation;
    public GemUI[,] GemUIs;
    public MagicItem MyMagicItem;
    public Simplest MetaLevel;
    public bool DoMouseEvent;
    public bool SimParticles;
    private class ParticleAndTrail
    {
        public CircuitUI Parent;
        public Particle MyParticle;
        public ManaTrail MyTrail;
        public Vector2 Follower;
        public ParticleAndTrail(
            CircuitUI parent,
            PointInt sourceLocation, Gem.Source source
        )
        {
            Parent = parent;
            MyParticle = new Particle(
                sourceLocation, source.Direction, Simplest.Ones(1)
            );
            Follower = sourceLocation.ToVector2();
            MyTrail = new ManaTrail();
            parent._container.AddChild(MyTrail);
            MyTrail.LineWidth = (float)(3 * Math.Exp(-parent.RecursionDepth));
            MyTrail.Lifetime = .5f;
        }
        public void Follow()
        {
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
    public CircuitUI(
        MagicItem magicItem, int recursionDepth,
        bool doMouseEvent, bool simParticles
    ) : base()
    {
        MyMagicItem = magicItem;
        RecursionDepth = recursionDepth;
        DoMouseEvent = doMouseEvent;
        SimParticles = simParticles;
        switch (MyMagicItem)
        {
            case Wand wand:
                MetaLevel = new Simplest(Rank.FINITE, -1);
                MyCircuit = wand.MyCircuit;
                break;
            case CustomGem cG:
                MetaLevel = cG.MetaLevel;
                MyCircuit = cG.MyCircuit;
                break;
            case Gem _:
            default:
                throw new Shared.TypeError();
        }
        _pAndTs = new List<ParticleAndTrail>();
        _pAndTsToFree = new Queue<ParticleAndTrail>();
        GemUIs = new GemUI[
            MyCircuit.Size.IntX,
            MyCircuit.Size.IntY
        ];

        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        _container = new MarginContainer();
        AddChild(_container);
        _bgRect = new Control();
        _grid = new GridContainer();
        _container.AddChild(_grid);
        _grid.AddConstantOverride("vseparation", 0);
        _grid.AddConstantOverride("hseparation", 0);

        Rebuild();
    }

    public void Rebuild()
    {
        // Console.WriteLine("Rebuild start");
        Shared.QFreeChildren(_grid);
        _bgRect.QueueFree();
        SetBackGround();
        _container.AddChild(_bgRect);
        _container.MoveChild(_bgRect, 0);
        Ratio = MyCircuit.Size.IntX / (float)MyCircuit.Size.IntY;
        if (RecursionDepth > Shared.MAX_RECURSION)
            return;
        _grid.Columns = MyCircuit.Size.IntX;
        if (RecursionDepth >= 1)
        {
            _grid.Columns -= 2;
        }
        for (int j = 0; j < MyCircuit.Size.IntY; j++)
        {
            for (int i = 0; i < MyCircuit.Size.IntX; i++)
            {
                if (RecursionDepth >= 1 && (
                    j == 0 || j == MyCircuit.Size.IntY - 1 ||
                    i == 0 || i == MyCircuit.Size.IntX - 1
                ))
                {
                    continue;
                }
                Gem gem = MyCircuit.Field[i, j];
                GemUI gemUI = new GemUI(
                    gem, RecursionDepth,
                    MetaLevel >= Simplest.Zero(), SimParticles
                );
                _grid.AddChild(gemUI);
                GemUIs[i, j] = gemUI;
                if (DoMouseEvent && RecursionDepth == 0)
                {
                    gemUI.Button.Connect(
                        "pressed", this, "OnClickGem",
                        new Godot.Collections.Array() { i, j }
                    );
                    gemUI.ConnectMouseOver();
                    gemUI.Connect(
                        "mouse_entered_overlay", this, "MouseEnteredGem",
                        new Godot.Collections.Array() { i, j }
                    );
                    gemUI.Connect(
                        "mouse_exited_overlay", this, "MouseExitedGem",
                        new Godot.Collections.Array() { i, j }
                    );
                }
            }
        }
        // Console.WriteLine("Rebuild end");
    }

    public override void _Process(float delta)
    {
        if (
            !SimParticles
            || RecursionDepth > Shared.MAX_RECURSION
        )
            return;
        // Console.WriteLine("Process begin");
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
            List<(PointInt, Gem.Source)> sources = MyCircuit.FindAll<Gem.Source>();
            var (sourceLocation, source) = sources[Shared.Rand.Next(sources.Count)];
            ParticleAndTrail pT = new ParticleAndTrail(this, sourceLocation, source);
            _pAndTs.Add(pT);
        }
        // Console.WriteLine("Process end");
    }

    private Vector2 ToUICoords(Vector2 circuitCoords)
    {
        Godot.Collections.Array a = _grid.GetChildren();
        if (a.Count == 0)
        {
            Console.WriteLine("count == 0");
            Console.WriteLine(Name);
        }
        int unit = (int)(_grid.GetChildren()[0] as GemUI).RectSize.x;
        if (RecursionDepth == 0)
            return (circuitCoords + HALF) * unit;
        return (circuitCoords - HALF) * unit;
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
        Shared.Assert(RecursionDepth == 0);
        _GemList = new GemListScene();
        AddChild(_GemList);
        _GemList.Connect(
            "finished", this, "onGemListFinish"
        );
        _GemList.ListPlacable(MetaLevel);
        _GemList.PopupCentered();
    }

    public void onGemListFinish()
    {
        if (_GemList.Selected == null)
            return;
        MyCircuit.Remove(_selectedLocation);
        Gem gem = (Gem)_GemList.Selected;
        if (!(gem is Gem.RemoveGem))
        {
            MyCircuit.Add(gem, _selectedLocation);
            _GemList.Selected = null;
        }
        Rebuild();
        EmitSignal("modified");
    }
    private void SetBackGround()
    {
        if (MetaLevel.MyRank != Rank.FINITE)
        {
            TextureRect _tRect = new TextureRect();
            _tRect.Texture = GD.Load<Texture>("res://texture/gem/wall.png");
            _tRect.Expand = true;
            _tRect.StretchMode = TextureRect.StretchModeEnum.Scale;
            ShaderMaterial mat = new ShaderMaterial();
            mat.Shader = _rainbow;
            _tRect.Material = mat;
            _bgRect = _tRect;
            return;
        }
        if (MyMagicItem is Wand wand)
        {
            TextureRect _tRect = new TextureRect();
            _tRect.Texture = wand.TextureFlat();
            _tRect.Expand = true;
            _tRect.StretchMode = TextureRect.StretchModeEnum.Scale;
            _bgRect = _tRect;
            return;
        }
        // Typed CG
        Color color;
        switch (MetaLevel.K % 3)
        {
            case 0:
                color = Color.FromHsv(.66f, 1f, .3f, 1f);
                break;
            case 1:
                color = Color.FromHsv(.38f, 1f, .3f, 1f);
                break;
            case 2:
                color = Color.FromHsv(0f, 1f, .3f, 1f);
                break;
            default:
                throw new Shared.ValueError();
        }
        ColorRect _cRect = new ColorRect();
        _cRect.Color = color;
        _bgRect = _cRect;
    }

    public void MouseEnteredGem(int i, int j)
    {
        Gem gem = MyCircuit.Field[i, j];
        string explain;
        if (gem == null)
        {
            explain = "No gem here. Click to add!";
        }
        else
        {
            explain = gem.Explain(MyMagicItem is CustomGem);
        }
        EmitSignal("new_explain", explain);

    }
    public void MouseExitedGem(int i, int j)
    {
        EmitSignal("new_explain", "");
    }
}
