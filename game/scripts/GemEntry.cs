using Godot;
using System;

public class GemEntry : HBoxContainer
{
    // code-defined
    private static readonly int SIZE = 80;
    public GemUI MyGemUI;
    public RichTextLabel[] Labels;
    public GemEntry() : base() { }
    public GemEntry(Gem gem) : base()
    {
        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        RectMinSize = new Vector2(0, SIZE);
        MyGemUI = new GemUI(gem);
        AddChild(MyGemUI);
        MyGemUI.RectMinSize = new Vector2(SIZE, SIZE);
        MyGemUI.SizeFlagsHorizontal = (int)Container.SizeFlags.Fill;
        MyGemUI.SizeFlagsVertical = (int)Container.SizeFlags.Fill;
        Labels = new RichTextLabel[6];
        for (int i = 0; i < 6; i++)
        {
            Labels[i] = new RichTextLabel();
            AddChild(Labels[i]);
            Labels[i].RectMinSize = new Vector2(45, 0);
            Labels[i].BbcodeEnabled = true;
            Labels[i].ScrollActive = false;
            Labels[i].SizeFlagsVertical = (int)Container.SizeFlags.ShrinkCenter;
            Labels[i].FitContentHeight = true;
        }
        Labels[5].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
    }
}
