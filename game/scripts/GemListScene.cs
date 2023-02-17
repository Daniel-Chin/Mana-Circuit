using Godot;
using System;
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
        GemEntry gemEntry = new GemEntry();
        MaskButton maskButton = new MaskButton(gemEntry);
        MyVBox.AddChild(maskButton);
        gemEntry.MyGemUI.Set(gem);
        maskButton.Mask.Connect(
            "pressed", this, "OnClickGem",
            new Godot.Collections.Array() { gemI }
        );
        if (gem == null)
        {
            gemEntry.Labels[5].Text = "Empty space.";
            return;
        }
        gemEntry.Labels[0].BbcodeText = "[color=lime][center]0[/center][/color]";
        gemEntry.Labels[1].BbcodeText = "[center]/[/center]";
        gemEntry.Labels[2].BbcodeText = "[color=aqua][center]0[/center][/color]";
        gemEntry.Labels[3].BbcodeText = "[center]/[/center]";
        gemEntry.Labels[4].BbcodeText = "[color=yellow][center]0[/center][/color]";
        gemEntry.Labels[5].BbcodeText = gem.Explain();
    }

    public void ListAll()
    {
        _rotated = false;
        Clear();
        // headder
        GemEntry gemEntry = new GemEntry();
        MyVBox.AddChild(gemEntry);
        gemEntry.MyGemUI.Empty();
        // gemEntry.RectMinSize = new Vector2(0, 100);
        gemEntry.Labels[0].BbcodeText = (
            "[center][color=lime]Available[/color] /\n"
            + "[color=aqua]In {wand}[/color] /\n"
            + "[color=yellow]In Custom Gems[/color][/center]"
        );
        gemEntry.Labels[0].RectMinSize = new Vector2(
            gemEntry.Labels[0].RectMinSize.x * 5, 0
        );
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
    public override void _Process(float delta)
    {
        Console.WriteLine(GetNode<ScrollContainer>("MyDialog/TabContainer/Gems").RectSize);
    }
}
