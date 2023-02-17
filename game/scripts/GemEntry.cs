using Godot;
using System;

public class GemEntry : HBoxContainer
{
    public GemUI MyGemUI;
    public Label[] Labels;
    public GemEntry() : base()
    {
        Shared.QFreeChildren(this);
        MyGemUI = GemUI.ThisScene.Instance<GemUI>();
        AddChild(MyGemUI);
        MyGemUI.RectMinSize = new Vector2(80, 80);
        MyGemUI.SizeFlagsHorizontal = (int)Container.SizeFlags.Fill;
        MyGemUI.SizeFlagsVertical = (int)Container.SizeFlags.Fill;
        Labels = new Label[4];
        for (int i = 0; i < 4; i++)
        {
            Labels[i] = new Label();
            AddChild(Labels[i]);
            Labels[i].RectMinSize = new Vector2(30, 0);
            Labels[i].Align = Label.AlignEnum.Center;
        }
        Labels[3].SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        Labels[3].Autowrap = true;
    }
}
