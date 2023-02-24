using Godot;

public class HPad : HBoxContainer
{
    private int _padding;

    public HPad(Control child) : this(child, 10) { }
    public HPad(Control child, int padding) : base()
    {
        _padding = padding;
        AddChild(Pad());
        child.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        child.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        AddChild(child);
        AddChild(Pad());
    }

    private MarginContainer Pad()
    {
        MarginContainer pad = new MarginContainer();
        pad.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        pad.RectMinSize = new Vector2(_padding, 0);
        return pad;
    }
}
