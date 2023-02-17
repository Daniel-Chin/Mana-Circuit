using Godot;
using System;

public class MaskButton : MarginContainer
{
    // code-defined
    private static readonly Texture _normal = GD.Load<Texture>("res://texture/maskButton/normal.png");
    private static readonly Texture _pressed = GD.Load<Texture>("res://texture/maskButton/pressed.png");
    private static readonly Texture _hover = GD.Load<Texture>("res://texture/maskButton/hover.png");
    public TextureButton Mask;
    public MaskButton()
    {
        // This is for the Godot editor, I guess?
        // Do nothing
    }
    public MaskButton(Control child)
    {
        AddChild(new PanelContainer());
        // Just for its theme. Cannot be Panel. 
        AddChild(child);
        Mask = new TextureButton();
        AddChild(Mask);
        Mask.Expand = true;
        Mask.StretchMode = TextureButton.StretchModeEnum.Tile;
        Mask.TextureNormal = _normal;
        Mask.TextureHover = _hover;
        Mask.TexturePressed = _pressed;
    }
}
