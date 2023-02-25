using Godot;
using System;
using System.Linq;

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
        MyGemUI = new GemUI(gem, 1, false, true);
        AddChild(MyGemUI);
        MyGemUI.RectMinSize = new Vector2(SIZE, SIZE);
        MyGemUI.SizeFlagsHorizontal = (int)Container.SizeFlags.Fill;
        MyGemUI.SizeFlagsVertical = (int)Container.SizeFlags.Fill;
    }

    private RichTextLabel OneLabel(int i)
    {
        Labels[i] = new RichTextLabel();
        AddChild(Labels[i]);
        Labels[i].RectMinSize = new Vector2(SMALL, 0);
        Labels[i].BbcodeEnabled = true;
        Labels[i].ScrollActive = false;
        Labels[i].SizeFlagsVertical = (int)Container.SizeFlags.ShrinkCenter;
        Labels[i].FitContentHeight = true;
        return Labels[i];
    }

    private static readonly int SMALL = 45;
    private static readonly int MONEY_WIDTH = 100;
    public void PresetFiveSmallOneBig()
    {
        Labels = new RichTextLabel[6];
        OneLabel(0);
        OneLabel(1);
        OneLabel(2);
        OneLabel(3);
        OneLabel(4);
        OneLabel(5).SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
    }
    public void PresetFiveSmallMerged()
    {
        Labels = new RichTextLabel[1];
        OneLabel(0).RectMinSize = new Vector2(SMALL * 5, 0);
    }
    public void PresetOneBig()
    {
        Labels = new RichTextLabel[1];
        OneLabel(0).SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
    }
    public void PresetBigSmallMoney()
    {
        Labels = new RichTextLabel[3];
        OneLabel(0).SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        OneLabel(1).RectMinSize = new Vector2(MONEY_WIDTH, 0);
        OneLabel(2);
    }
    public void Pad()
    {
        MarginContainer padder = new MarginContainer();
        AddChild(padder);
        MoveChild(padder, 1);
        padder.RectMinSize = new Vector2(30, 0);
    }
}
