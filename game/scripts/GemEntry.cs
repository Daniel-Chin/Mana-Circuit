using Godot;
using System;
using System.Linq;

public class GemEntry : HBoxContainer
{
    // code-defined
    public static readonly int N_LABELS = 6;
    private static readonly int SIZE = 80;
    public GemUI MyGemUI;
    public RichTextLabel[] Labels;
    public GemEntry() : base() { }
    public GemEntry(Gem gem) : base()
    {
        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        RectMinSize = new Vector2(0, SIZE);
        MyGemUI = new GemUI(gem, 1, false);
        AddChild(MyGemUI);
        MyGemUI.RectMinSize = new Vector2(SIZE, SIZE);
        MyGemUI.SizeFlagsHorizontal = (int)Container.SizeFlags.Fill;
        MyGemUI.SizeFlagsVertical = (int)Container.SizeFlags.Fill;
        Labels = new RichTextLabel[N_LABELS];
        for (int i = 0; i < N_LABELS; i++)
        {
            Labels[i] = new RichTextLabel();
            AddChild(Labels[i]);
            Labels[i].RectMinSize = new Vector2(45, 0);
            Labels[i].BbcodeEnabled = true;
            Labels[i].ScrollActive = false;
            Labels[i].SizeFlagsVertical = (int)Container.SizeFlags.ShrinkCenter;
            Labels[i].FitContentHeight = true;
        }
        Labels[N_LABELS - 1].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
    }

    public void ExpandFirstLabel(bool excludeLastLabel)
    {
        if (excludeLastLabel)
        {
            Labels[0].RectMinSize = new Vector2(
                Labels[0].RectMinSize.x * (N_LABELS - 1), 0
            );
        }
        else
        {
            MarginContainer padder = new MarginContainer();
            AddChild(padder);
            MoveChild(padder, 1);
            padder.RectMinSize = new Vector2(30, 0);
            Labels[0].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        }
        foreach (var label in Labels.Skip(1))
        {
            label.QueueFree();
        }
    }
}
