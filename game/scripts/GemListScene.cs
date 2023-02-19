using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class GemListScene : WindowDialog
{
    // code-defined
    [Signal] public delegate void gemSelected();
    private static readonly Vector2 SIZE = new Vector2(800, 500);
    public VBoxContainer GemVBox;
    public VBoxContainer CGVBox;
    private List<Gem> _gems;
    public Gem SelectedGem;
    public Simplest MetaLevel;
    private bool _rotated;

    public GemListScene() : base()
    {
        _gems = new List<Gem>();
        TabContainer tabs = new TabContainer();
        AddChild(tabs);
        ScrollContainer gemScroll = new ScrollContainer();
        ScrollContainer cgScroll = new ScrollContainer();
        tabs.AddChild(gemScroll);
        tabs.AddChild(cgScroll);
        GemVBox = new VBoxContainer();
        CGVBox = new VBoxContainer();
        gemScroll.AddChild(GemVBox);
        cgScroll.AddChild(CGVBox);
        RectMinSize = SIZE;
        Vector2 shrinked = new Vector2(SIZE.x - 8, SIZE.y - 51);
        gemScroll.RectMinSize = shrinked;
        cgScroll.RectMinSize = shrinked;
        GemVBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        GemVBox.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        CGVBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        CGVBox.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        tabs.SetTabTitle(0, "Gems");
        tabs.SetTabTitle(1, "Custom Gems");
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
        if (gem == null)
        {
            gemEntry.Labels[5].Text = "Remove gem.";
            return;
        }
        if (_rotated)
        {
            gemEntry.Labels[0].BbcodeText = "[center]This way![/center]";
            gemEntry.Labels[0].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
            foreach (var label in gemEntry.Labels.Skip(1))
            {
                label.QueueFree();
            }
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
        _gems.Clear();
        MetaLevel = metaLevel;
        ListAll(GemVBox);
        ListAll(CGVBox);
    }
    public void ListAll(VBoxContainer vBox)
    {
        _rotated = false;
        Shared.QFreeChildren(vBox);

        // header
        GemEntry gemEntry = new GemEntry(null);
        vBox.AddChild(gemEntry);
        gemEntry.MyGemUI.Empty();
        gemEntry.Labels[0].BbcodeText = (
            "[center][color=lime]Available[/color] /\n"
            + "[color=aqua]In {wand}[/color] /\n"
            + "[color=yellow]In Custom Gems[/color][/center]"
        );
        gemEntry.Labels[0].RectMinSize = new Vector2(
            gemEntry.Labels[0].RectMinSize.x * 5, 0
        );
        foreach (var label in gemEntry.Labels.Skip(1))
        {
            label.QueueFree();
        }
        // contents
        if (vBox == GemVBox)
        {
            Add(vBox, null);
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
                Add(vBox, entry.Value);
            }
        }
    }

    private bool AskRotate()
    {
        Gem gem = SelectedGem;
        _gems.Clear();
        Shared.QFreeChildren(GemVBox);
        // header
        GemEntry gemEntry = new GemEntry(null);
        GemVBox.AddChild(gemEntry);
        gemEntry.MyGemUI.Empty();
        gemEntry.Labels[0].BbcodeText = "[center]Which way?[/center]";
        gemEntry.Labels[0].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        foreach (var label in gemEntry.Labels.Skip(1))
        {
            label.QueueFree();
        }
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
        SelectedGem = _gems[gemI];
        if (SelectedGem != null)
        {
            var (nInWand, nInCGs, nAvailable) = CountGems(SelectedGem);
            if (nAvailable <= Simplest.Zero())
            {
                // No gem available
                return;
            }
            if (
                SelectedGem is CustomGem cG && (
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
        Visible = false;
        Shared.QFreeChildren(GemVBox);
        Shared.QFreeChildren(CGVBox);
        EmitSignal("gemSelected");
    }

    private Simplest CountGemsOwned(Gem gem)
    {
        if (gem is CustomGem cG)
        {
            CustomGem HasCG = null;
            if (cG.MetaLevel.MyRank == Rank.FINITE)
            {
                GameState.Persistent.HasCustomGems.TryGetValue(
                    (int)cG.MetaLevel.K, out HasCG
                );
                if (HasCG == null)
                    return Simplest.Zero();
                return Simplest.One();
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
        foreach (Gem g in circuit.Gems)
        {
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
            cG = entry.Value;
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
}
