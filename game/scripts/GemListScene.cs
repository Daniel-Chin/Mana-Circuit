using Godot;
using System;
using System.Collections.Generic;

public class GemListScene : WindowDialog
{
    // code-defined
    [Signal] public delegate void finished();
    private static readonly Vector2 SIZE = new Vector2(800, 500);
    public VBoxContainer OuterVBox;
    public Label Title;
    public ScrollContainer MyScroll;
    public VBoxContainer VBox;
    private List<Gem> _gems;
    public Simplest MetaLevel;
    public MagicItem Selected;
    private bool _rotated;

    public GemListScene() : base()
    {
        _gems = new List<Gem>();
        Connect("popup_hide", this, "OnPopupHide");

        Theme = Shared.THEME;
        RectMinSize = SIZE;
        OuterVBox = new VBoxContainer();
        AddChild(OuterVBox);
        OuterVBox.RectMinSize = SIZE;
        Title = new Label();
        OuterVBox.AddChild(Title);
        Title.Align = Label.AlignEnum.Center;
        Title.Valign = Label.VAlign.Center;
        Title.RectMinSize = new Vector2(0, 50);
        MyScroll = new ScrollContainer();
        OuterVBox.AddChild(MyScroll);
        MyScroll.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        VBox = new VBoxContainer();
        MyScroll.AddChild(VBox);
        VBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
    }

    private GemEntry Add(Gem gem)
    {
        int gemI = _gems.Count;
        _gems.Add(gem);
        GemEntry gemEntry = new GemEntry(gem);
        MaskButton maskButton = new MaskButton(gemEntry);
        VBox.AddChild(maskButton);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickGem",
            new Godot.Collections.Array() { gemI }
        );
        return gemEntry;
    }

    private void FillEntryRemove(GemEntry gemEntry, Gem gem)
    {
        gemEntry.Labels[5].BbcodeText = gem.Explain(false);
    }
    private void FillEntryThisWay(GemEntry gemEntry, Gem gem)
    {
        gemEntry.ExpandFirstLabel(false);
        gemEntry.Labels[0].BbcodeText = "[center]This way![/center]";
    }
    private void FillEntryInStock(GemEntry gemEntry, Gem gem)
    {
        var (nInWand, nInCGs, nAvailable) = CountGems(gem);
        gemEntry.Labels[0].BbcodeText = $"[color=lime][center]{MathBB.Build(nAvailable)}[/center][/color]";
        gemEntry.Labels[1].BbcodeText = "[center]/[/center]";
        gemEntry.Labels[2].BbcodeText = $"[color=aqua][center]{nInWand}[/center][/color]";
        gemEntry.Labels[3].BbcodeText = "[center]/[/center]";
        gemEntry.Labels[4].BbcodeText = $"[color=yellow][center]{nInCGs}[/center][/color]";
        gemEntry.Labels[5].BbcodeText = gem.Explain(false);
    }

    public void ListPlacable(Simplest metaLevel)
    {
        MetaLevel = metaLevel;
        _rotated = false;

        Title.Text = "Which gem?";
        // header
        GemEntry gemEntry = new GemEntry(null);
        OuterVBox.AddChild(gemEntry);
        OuterVBox.MoveChild(gemEntry, 1);
        gemEntry.MyGemUI.Empty();
        string wandName = GameState.Persistent.MyWand.DisplayName();
        gemEntry.Labels[0].BbcodeText = (
            "[center][color=lime]Available[/color] /\n"
            + $"[color=aqua]In {wandName}[/color] /\n"
            + "[color=yellow]In Custom Gems[/color][/center]"
        );
        gemEntry.ExpandFirstLabel(true);

        ListAllNonCG();
        ListAllCGs();
    }
    public void ListAllNonCG()
    {
        Gem gem;
        GemEntry gemEntry;
        int n;

        gem = new Gem.RemoveGem();
        gemEntry = Add(gem);
        FillEntryRemove(gemEntry, gem);

        if (
            GameState.Persistent.HasGems.TryGetValue("addOne", out n)
            && n != 0
        )
        {
            gem = new Gem.AddOne();
            gemEntry = Add(gem);
            FillEntryInStock(gemEntry, gem);
        }
        if (
            GameState.Persistent.HasGems.TryGetValue("weakMult", out n)
            && n != 0
        )
        {
            gem = new Gem.WeakMult();
            gemEntry = Add(gem);
            FillEntryInStock(gemEntry, gem);
        }
        if (
            GameState.Persistent.HasGems.TryGetValue("focus", out n)
            && n != 0
        )
        {
            gem = new Gem.Focus(new PointInt(0, 1));
            gemEntry = Add(gem);
            FillEntryInStock(gemEntry, gem);
        }
        if (
            GameState.Persistent.HasGems.TryGetValue("mirror", out n)
            && n != 0
        )
        {
            gem = new Gem.Mirror(true);
            gemEntry = Add(gem);
            FillEntryInStock(gemEntry, gem);
        }
        if (
            GameState.Persistent.HasGems.TryGetValue("stochastic", out n)
            && n != 0
        )
        {
            gem = new Gem.Stochastic(true);
            gemEntry = Add(gem);
            FillEntryInStock(gemEntry, gem);
        }
    }

    public void ListAllCGs()
    {
        GemEntry gemEntry;
        if (GameState.Persistent.MyTypelessGem is CustomGem typeless)
        {
            gemEntry = Add(typeless);
            FillEntryInStock(gemEntry, typeless);
        }
        foreach (var entry in GameState.Persistent.HasCustomGems)
        {
            var (n, cG) = entry.Value;
            if (n != 0)
            {
                gemEntry = Add(cG);
                FillEntryInStock(gemEntry, cG);
            }
        }
    }

    private bool AskRotate()
    {
        Gem gem = (Gem)Selected;
        _gems.Clear();
        Shared.QFreeChildren(VBox);
        Title.Text = "Which way?";
        switch (gem)
        {
            case Gem.Focus _:
                Add(new Gem.Focus(new PointInt(1, 0)));
                Add(new Gem.Focus(new PointInt(-1, 0)));
                Add(new Gem.Focus(new PointInt(0, 1)));
                Add(new Gem.Focus(new PointInt(0, -1)));
                break;
            case Gem.Stochastic _:
                Add(new Gem.Stochastic(true));
                Add(new Gem.Stochastic(false));
                break;
            case Gem.Mirror _:
                Add(new Gem.Mirror(true));
                Add(new Gem.Mirror(false));
                break;
            default:
                return false;
        }
        return true;
    }

    public void OnClickGem(int gemI)
    {
        Gem gem = _gems[gemI];
        CustomGem cG = gem as CustomGem;
        if (!(gem is Gem.RemoveGem))
        {
            var (nInWand, nInCGs, nAvailable) = CountGems(gem);
            if (nAvailable <= Simplest.Zero())
            {
                // No gem available
                return;
            }
            if (
                cG != null
                && cG.MetaLevel >= MetaLevel
                && cG.MetaLevel.MyRank == Rank.FINITE
            )
            {
                // Gem type illegal
                return;
            }
        }
        Selected = (_gems[gemI]);
        if (cG == null && !_rotated)
        {
            _rotated = true;
            if (AskRotate())
                return;
        }
        Hide();
    }

    private Simplest CountGemsOwned(Gem gem)
    {
        if (gem is CustomGem cG)
        {
            (int, CustomGem) HasCG = (0, null);
            if (cG.MetaLevel.MyRank == Rank.FINITE)
            {
                if (GameState.Persistent.HasCustomGems.TryGetValue(
                    (int)cG.MetaLevel.K, out HasCG
                ))
                    return new Simplest(Rank.FINITE, HasCG.Item1);
                return Simplest.Zero();
            }
            else
            {
                // typeless
                if (GameState.Persistent.MyTypelessGem == null)
                    return Simplest.Zero();
                return Simplest.W();
            }
        }
        int n = 0;
        if (!GameState.Persistent.HasGems.TryGetValue(gem.Name(), out n))
            n = 0;
        return new Simplest(Rank.FINITE, n);
    }

    private int CountGemsInCircuit(Gem gem, Circuit circuit)
    {
        int acc = 0;
        foreach (Gem g in circuit.Field)
        {
            if (g == null) continue;
            if (gem is CustomGem customGem)
            {
                if (g is CustomGem cG)
                {
                    if (cG.MetaLevel.Equals(customGem.MetaLevel))
                        acc++;
                }
            }
            else
            {
                if (g.GetType() == gem.GetType())
                    acc++;
            }
        }
        return acc;
    }
    private int CountGemsInWand(Gem gem)
    {
        return CountGemsInCircuit(gem, GameState.Persistent.MyWand.MyCircuit);
    }
    private int CountGemsInCGs(Gem gem)
    {
        int acc = 0;
        CustomGem cG;
        foreach (var entry in GameState.Persistent.HasCustomGems)
        {
            cG = entry.Value.Item2;
            if (cG != null)
            {
                acc += CountGemsInCircuit(gem, cG.MyCircuit);
            }
        }
        cG = GameState.Persistent.MyTypelessGem;
        if (cG != null)
        {
            acc += CountGemsInCircuit(gem, cG.MyCircuit);
        }
        return acc;
    }

    public (int, int, Simplest) CountGems(Gem gem)
    {
        int nInWand = CountGemsInWand(gem);
        int nInCGs = CountGemsInCGs(gem);
        Simplest nOwned = CountGemsOwned(gem);
        Simplest nAvailable;
        if (nOwned.MyRank == Rank.FINITE)
        {
            nAvailable = new Simplest(
                Rank.FINITE, nOwned.K - nInWand - nInCGs
            );
        }
        else
        {
            nAvailable = nOwned;
        }
        return (nInWand, nInCGs, nAvailable);
    }

    public void ListEditables()
    {
        Title.Text = "Which one to design?";
        MetaLevel = Simplest.W();

        Wand wand = GameState.Persistent.MyWand;
        GemEntry gemEntry = new GemEntry(null);
        MaskButton maskButton = new MaskButton(gemEntry);
        VBox.AddChild(maskButton);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickWand"
        );
        gemEntry.MyGemUI.Button.TextureNormal = wand.Texture();
        gemEntry.ExpandFirstLabel(false);
        gemEntry.Labels[0].BbcodeText = wand.DisplayName();

        ListAllCGs();
    }

    public void OnClickWand()
    {
        Selected = GameState.Persistent.MyWand;
        Hide();
    }

    private void Finish()
    {
        EmitSignal("finished");
        QueueFree();
    }
    public void OnPopupHide()
    {
        Finish();
    }
}
