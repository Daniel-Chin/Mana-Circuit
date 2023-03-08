using Godot;
using System;

using System.Collections.Generic;

public class CircuitUI : AspectRatioContainer
{
    // code-defined
    [Signal] public delegate void new_explain();
    [Signal] public delegate void modified();
    private static readonly int MAX_PARTICLES = 8;
    private static readonly float EMIT_INTERVAL = 1f;
    private static readonly double ADVECT_LAMBDA = 8;
    // poisson distribution, prob / sec
    private static readonly float ADVECT_LEN = .3f;
    private static readonly float NOISE_MULT = .5f;
    private static readonly Vector2 HALF = new Vector2(.5f, .5f);
    private static Shader _rainbow = GD.Load<Shader>("res://Rainbow.gdshader");
    public Circuit MyCircuit;
    public int RecursionDepth;
    private MarginContainer _container;
    private Control _bgRect;
    private MarginContainer _gridWrapper;
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
    private float _emitAcc;
    public Image Screenshot {get; set;}
    public Dictionary<Simplest, CircuitUI> ScreenshotCache {get; set;}
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
            MyTrail = new ManaTrail(parent.MyMagicItem is Wand);
            parent._gridWrapper.AddChild(MyTrail);
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
            MyTrail.SetMana(MyParticle.Mana[0]);
        }
        public void Free()
        {
            MyTrail.QueueFree();
        }
    }
    public CircuitUI() : base() { }
    public CircuitUI(
        MagicItem magicItem, int recursionDepth,
        bool doMouseEvent, bool simParticles, 
        Dictionary<Simplest, CircuitUI> screenshotCache
    ) : base()
    {
        MyMagicItem = magicItem;
        RecursionDepth = recursionDepth;
        DoMouseEvent = doMouseEvent;
        SimParticles = simParticles;
        Screenshot = null;
        ScreenshotCache = screenshotCache;
        bool isTypedCG = false;
        switch (MyMagicItem)
        {
            case Wand wand:
                MetaLevel = new Simplest(Rank.STACK_W, 3);
                MyCircuit = wand.MyCircuit;
                break;
            case CustomGem cG:
                MetaLevel = cG.MetaLevel;
                MyCircuit = cG.MyCircuit;
                if (MetaLevel.MyRank == Rank.FINITE)
                    isTypedCG = true;
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
        _grid.AddConstantOverride("vseparation", 0);
        _grid.AddConstantOverride("hseparation", 0);
        _gridWrapper = new MarginContainer();
        _gridWrapper.AddChild(_grid);
        if (isTypedCG) {
            RatioCenter(_gridWrapper, _container);
        } else {
            _container.AddChild(_gridWrapper);
        }
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
                    !(MyMagicItem is Wand), SimParticles, 
                    ScreenshotCache
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
        foreach (GemUI gemUI in GemUIs) {
            if (gemUI != null)
                gemUI.PaintIn();
        }
        // Console.WriteLine("Rebuild end");
    }

    public override void _Process(float delta)
    {
        Screenshot = Main.Singleton.ScreenshotRect(_container);
        if (
            !SimParticles
            || RecursionDepth > Shared.MAX_RECURSION
        )
            return;
        // Console.WriteLine("Process begin");
        // advect
        foreach (ParticleAndTrail pT in _pAndTs)
        {
            if (Shared.Rand.NextDouble() < ADVECT_LAMBDA * delta)
                pT.Follow();
        }
        // free
        while (_pAndTsToFree.Count != 0)
        {
            ParticleAndTrail pT = _pAndTsToFree.Dequeue();
            pT.Free();
            _pAndTs.Remove(pT);
        }
        // emit
        _emitAcc += delta;
        if (
            _pAndTs.Count < MAX_PARTICLES
            && _emitAcc >= EMIT_INTERVAL
        )
        {   
            _emitAcc -= EMIT_INTERVAL;
            // spawn new particle
            // Console.WriteLine("spawn new particle");
            List<(PointInt, Gem.Source)> sources = MyCircuit.FindAll<Gem.Source>();
            foreach (var (sourceLocation, source) in sources)
            {
                _pAndTs.Add(new ParticleAndTrail(
                    this, sourceLocation, source
                ));
            }
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
        if (MyMagicItem is Wand wand)
        {
            TextureRect tRect = new TextureRect();
            tRect.Texture = wand.TextureFlat();
            tRect.Expand = true;
            tRect.StretchMode = TextureRect.StretchModeEnum.Scale;
            _bgRect = tRect;
            return;
        }
        if (MetaLevel.MyRank != Rank.FINITE)
        {
            TextureRect tRect = new TextureRect();
            tRect.Texture = GD.Load<Texture>("res://texture/gem/wall.png");
            tRect.Expand = true;
            tRect.StretchMode = TextureRect.StretchModeEnum.Scale;
            ShaderMaterial mat = new ShaderMaterial();
            mat.Shader = _rainbow;
            tRect.Material = mat;
            _bgRect = tRect;
            return;
        }
        {
            // Typed CG
            TextureRect tRect = new TextureRect();
            tRect.Texture = GD.Load<Texture>($"res://texture/gem/cg_{MetaLevel.K % 3}.png");
            tRect.Expand = true;
            _bgRect = tRect;
        }
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

    private static readonly float BORDER_WIDTH = .05f;
    private void RatioCenter(
        MarginContainer gridWrapper, Container container
    ) {
        VBoxContainer vBox = new VBoxContainer();
        HBoxContainer hBox = new HBoxContainer();
        container.AddChild(vBox);
        
        vBox.AddChild(Padder());
        vBox.AddChild(hBox);
        vBox.AddChild(Padder());
        hBox.AddChild(Padder());
        hBox.AddChild(gridWrapper);
        hBox.AddChild(Padder());

        hBox.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        gridWrapper.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
    }
    private MarginContainer Padder() {
        MarginContainer padder = new MarginContainer();
        padder.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        padder.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
        padder.SizeFlagsStretchRatio = BORDER_WIDTH;
        return padder;
    }
}
