using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class GemListScene : Node2D
{
    [Signal] public delegate void gemSelected();
    public WindowDialog MyDialog;
    public VBoxContainer MyVBox;
    private List<Gem> _gems;
    public Gem SelectedGem;
    private bool _rotated;

    public override void _Ready()
    {
        MyDialog = GetNode<WindowDialog>("MyDialog");
        MyVBox = GetNode<VBoxContainer>("MyDialog/TabContainer/Gems/MyVBox");
        _gems = new List<Gem>();
        // MyDialog.RectMinSize = new Vector2(WIDTH, 500);
        // GetNode<ScrollContainer>("MyDialog/TabContainer/Gems").RectMinSize = new Vector2(WIDTH, 500);
    }

    private void Clear()
    {
        Shared.QFreeChildren(MyVBox);
        _gems.Clear();
    }

    private void Add(Gem gem)
    {
        int gemI = _gems.Count;
        _gems.Add(gem);
        GemEntry gemEntry = new GemEntry(gem);
        MaskButton maskButton = new MaskButton(gemEntry);
        MyVBox.AddChild(maskButton);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickGem",
            new Godot.Collections.Array() { gemI }
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
        _rotated = false;
        Clear();
        // header
        GemEntry gemEntry = new GemEntry(null);
        MyVBox.AddChild(gemEntry);
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
        Add(null);
        Add(new Gem.AddOne());
        Add(new Gem.WeakMult());
        Add(new Gem.Focus(new PointInt(0, 1)));
        Add(new Gem.Mirror(true));
        Add(new Gem.Stochastic(true));
    }

    private bool AskRotate()
    {
        Gem gem = SelectedGem;
        Clear();
        // header
        GemEntry gemEntry = new GemEntry(null);
        MyVBox.AddChild(gemEntry);
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
        SelectedGem = _gems[gemI];
        if (!_rotated)
        {
            _rotated = true;
            if (AskRotate())
                return;
        }
        MyDialog.Visible = false;
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
