using Godot;

public class Pad
{
    public static HBoxContainer H(Control child, int padding)
    {
        HBoxContainer hBox = new HBoxContainer();
        Vector2 rect = new Vector2(padding, 0);
        Do(child, rect, hBox);
        return hBox;
    }
    public static VBoxContainer V(Control child, int padding)
    {
        VBoxContainer vBox = new VBoxContainer();
        Vector2 rect = new Vector2(0, padding);
        Do(child, rect, vBox);
        return vBox;
    }
    
    private static void Do(
        Control child, Vector2 rect, BoxContainer box
    ) {
        box.AddChild(Padder(rect));
        child.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        child.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        box.AddChild(child);
        box.AddChild(Padder(rect));
    }

    private static MarginContainer Padder(Vector2 rect)
    {
        MarginContainer pad = new MarginContainer();
        pad.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        pad.RectMinSize = rect;
        return pad;
    }
}
