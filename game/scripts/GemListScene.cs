using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class GemListScene : Node2D
{
    [Signal] public delegate void gemSelected();
    public WindowDialog MyDialog;
    public VBoxContainer GemVBox;
    public VBoxContainer CGVBox;
    private List<Gem> _gems;
    public Gem SelectedGem;
    private bool _rotated;

    public override void _Ready()
    {
        MyDialog = GetNode<WindowDialog>("MyDialog");
        GemVBox = GetNode<VBoxContainer>("MyDialog/TabContainer/Gems/MyVBox");
        CGVBox = GetNode<VBoxContainer>("MyDialog/TabContainer/Custom Gems/MyVBox");
        _gems = new List<Gem>();
        // MyDialog.RectMinSize = new Vector2(WIDTH, 500);
        // GetNode<ScrollContainer>("MyDialog/TabContainer/Gems").RectMinSize = new Vector2(WIDTH, 500);
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
            int nInWand = CountGemsInWand(gem);
            int nInCGs = CountGemsInCGs(gem);
            int nAvailable = CountGemsOwned(gem) - nInWand - nInCGs;
            gemEntry.Labels[0].BbcodeText = $"[color=lime][center]{nAvailable}[/center][/color]";
            gemEntry.Labels[1].BbcodeText = "[center]/[/center]";
            gemEntry.Labels[2].BbcodeText = $"[color=aqua][center]{nInWand}[/center][/color]";
            gemEntry.Labels[3].BbcodeText = "[center]/[/center]";
            gemEntry.Labels[4].BbcodeText = $"[color=yellow][center]{nInCGs}[/center][/color]";
            gemEntry.Labels[5].BbcodeText = gem.Explain();
        }
    }

    public void ListAll()
    {
        _gems.Clear();
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
        if (!gemIsCG && !_rotated)
        {
            _rotated = true;
            if (AskRotate())
                return;
        }
        MyDialog.Visible = false;
        Shared.QFreeChildren(GemVBox);
        Shared.QFreeChildren(CGVBox);
        EmitSignal("gemSelected");
    }

    private int CountGemsOwned(Gem gem)
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
                    return 0;
                return 1;
            }
            else
            {
                // typeless
                if (GameState.Persistent.MyTypelessGem == null)
                    return 0;
                return 1;
            }
        }
        int n = 0;
        GameState.Persistent.HasGems.TryGetValue(gem.Name(), out n);
        return n;
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
