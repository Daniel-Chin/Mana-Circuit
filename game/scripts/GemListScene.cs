using Godot;
using System;
using System.Collections.Generic;

public class GemListScene : WindowDialog
{
    // code-defined
    [Signal] public delegate void finished();
    private static readonly Vector2 SIZE = new Vector2(800, 500);
    public TabContainer Tabs;
    public ScrollContainer GemScroll;
    public ScrollContainer CGScroll;
    public VBoxContainer GemVBox;
    public VBoxContainer CGVBox;
    private List<Gem> _gems;
    public Simplest MetaLevel;
    public MagicItem Selected;
    private bool _rotated;

    public GemListScene() : base()
    {
        _gems = new List<Gem>();
        Tabs = new TabContainer();
        AddChild(Tabs);
        GemScroll = new ScrollContainer();
        CGScroll = new ScrollContainer();
        Tabs.AddChild(GemScroll);
        Tabs.AddChild(CGScroll);
        GemVBox = new VBoxContainer();
        CGVBox = new VBoxContainer();
        GemScroll.AddChild(GemVBox);
        CGScroll.AddChild(CGVBox);
        RectMinSize = SIZE;
        Vector2 shrinked = new Vector2(SIZE.x - 8, SIZE.y - 51);
        GemScroll.RectMinSize = shrinked;
        CGScroll.RectMinSize = shrinked;
        GemVBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        GemVBox.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        CGVBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        CGVBox.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        Tabs.SetTabTitle(0, "Gems");
        Tabs.SetTabTitle(1, "Custom Gems");
        Connect("popup_hide", this, "OnPopupHide");
    }

    private void Add(VBoxContainer vBox, Gem gem)
    {
        int gemI = _gems.Count;
        bool gemIsCG = vBox == CGVBox;
        _gems.Add(gem);
        GemEntry gemEntry = new GemEntry(gem);
        MaskButton maskButton = new MaskButton(gemEntry);
        vBox.AddChild(maskButton);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickGem",
            new Godot.Collections.Array() { gemIsCG, gemI }
        );
        if (gem is Gem.RemoveGem)
        {
            gemEntry.Labels[5].BbcodeText = gem.Explain();
            return;
        }
        if (_rotated)
        {
            gemEntry.ExpandFirstLabel(false);
            gemEntry.Labels[0].BbcodeText = "[center]This way![/center]";
        }
        else
        {
            var (nInWand, nInCGs, nAvailable) = CountGems(gem);
            gemEntry.Labels[0].BbcodeText = $"[color=lime][center]{MathBB.Build(nAvailable)}[/center][/color]";
            gemEntry.Labels[1].BbcodeText = "[center]/[/center]";
            gemEntry.Labels[2].BbcodeText = $"[color=aqua][center]{nInWand}[/center][/color]";
            gemEntry.Labels[3].BbcodeText = "[center]/[/center]";
            gemEntry.Labels[4].BbcodeText = $"[color=yellow][center]{nInCGs}[/center][/color]";
            gemEntry.Labels[5].BbcodeText = gem.Explain();
        }
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

    public void ListAll(Simplest metaLevel)
    {
        MetaLevel = metaLevel;
        ListAll(GemVBox);
        ListAll(CGVBox);
    }
    public void ListAll(VBoxContainer vBox)
    {
        _rotated = false;

        // header
        GemEntry gemEntry = new GemEntry(null);
        vBox.AddChild(gemEntry);
        gemEntry.MyGemUI.Empty();
        gemEntry.Labels[0].BbcodeText = (
            "[center][color=lime]Available[/color] /\n"
            + "[color=aqua]In {wand}[/color] /\n"
            + "[color=yellow]In Custom Gems[/color][/center]"
        );
        gemEntry.ExpandFirstLabel(true);
        // contents
        if (vBox == GemVBox)
        {
            Add(vBox, new Gem.RemoveGem());
            Add(vBox, new Gem.AddOne());
            Add(vBox, new Gem.WeakMult());
            Add(vBox, new Gem.Focus(new PointInt(0, 1)));
            Add(vBox, new Gem.Mirror(true));
            Add(vBox, new Gem.Stochastic(true));
        }
        else
        {
            if (GameState.Persistent.MyTypelessGem is CustomGem cG)
            {
                Add(vBox, cG);
            }
            foreach (var entry in GameState.Persistent.HasCustomGems)
            {
                Add(vBox, entry.Value.Item2);
            }
        }
    }

    private bool AskRotate()
    {
        Gem gem = (Gem)Selected;
        _gems.Clear();
        Shared.QFreeChildren(GemVBox);
        // header
        GemEntry gemEntry = new GemEntry(null);
        GemVBox.AddChild(gemEntry);
        gemEntry.MyGemUI.Empty();
        gemEntry.Labels[0].BbcodeText = "[center]Which way?[/center]";
        gemEntry.ExpandFirstLabel(false);
        // contents
        switch (gem)
        {
            case Gem.Focus _:
                Add(GemVBox, new Gem.Focus(new PointInt(1, 0)));
                Add(GemVBox, new Gem.Focus(new PointInt(-1, 0)));
                Add(GemVBox, new Gem.Focus(new PointInt(0, 1)));
                Add(GemVBox, new Gem.Focus(new PointInt(0, -1)));
                break;
            case Gem.Stochastic _:
                Add(GemVBox, new Gem.Stochastic(true));
                Add(GemVBox, new Gem.Stochastic(false));
                break;
            case Gem.Mirror _:
                Add(GemVBox, new Gem.Mirror(true));
                Add(GemVBox, new Gem.Mirror(false));
                break;
            default:
                return false;
        }
        return true;
    }

    public void OnClickGem(bool gemIsCG, int gemI)
    {
        Selected = (_gems[gemI]);
        if (!(Selected is Gem.RemoveGem))
        {
            var (nInWand, nInCGs, nAvailable) = CountGems((Gem)Selected);
            if (nAvailable <= Simplest.Zero())
            {
                // No gem available
                return;
            }
            if (
                Selected is CustomGem cG && (
                    cG.MetaLevel >= MetaLevel
                    && cG.MetaLevel.MyRank == Rank.FINITE
                )
            )
            {
                // Gem type illegal
                return;
            }
        }
        if (!gemIsCG && !_rotated)
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
                GameState.Persistent.HasCustomGems.TryGetValue(
                    (int)cG.MetaLevel.K, out HasCG
                );
                if (HasCG.Item2 == null)
                    return Simplest.Zero();
                return new Simplest(Rank.FINITE, HasCG.Item1);
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
        GameState.Persistent.HasGems.TryGetValue(gem.Name(), out n);
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

    public void ListEditables()
    {
        MetaLevel = Simplest.W();

        Wand wand = GameState.Persistent.MyWand;
        GemEntry gemEntry = new GemEntry(null);
        MaskButton maskButton = new MaskButton(gemEntry);
        CGVBox.AddChild(maskButton);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickWand"
        );
        gemEntry.MyGemUI.Button.TextureNormal = wand.Texture();
        gemEntry.ExpandFirstLabel(false);
        gemEntry.Labels[0].BbcodeText = wand.DisplayName();

        ListAll(CGVBox);

        Tabs.SetTabTitle(1, "Which one to design?");
        GemScroll.QueueFree();
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
