using Godot;
using System;
using System.Collections.Generic;

public class PocketScene : Node2D
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
        MyVBox = GetNode<VBoxContainer>("MyDialog/MyScroll/MyVBox");
        _gems = new List<Gem>();
    }

    private void Clear()
    {
        foreach (Node x in MyVBox.GetChildren())
        {
            x.QueueFree();
        }
        _gems.Clear();
    }

    private void Add(Gem gem)
    {
        int gemI = _gems.Count;
        _gems.Add(gem);
        GemUI gemUI = GemUI.ThisScene.Instance<GemUI>();
        gemUI.Set(gem);
        gemUI.Connect(
            "pressed", this, "OnClickGem",
            new Godot.Collections.Array() { gemI }
        );
        HBoxContainer hBox = new HBoxContainer();
        MyVBox.AddChild(hBox);
        hBox.AddChild(gemUI);
        hBox.RectMinSize = new Vector2(80, 80);
        hBox.RectSize = new Vector2(80, 80);
        hBox.SizeFlagsHorizontal = (int)Control.SizeFlags.Expand;
    }

    public void ListAll()
    {
        _rotated = false;
        Clear();
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
}
