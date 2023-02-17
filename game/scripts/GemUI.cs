using Godot;
using System;

public class GemUI : MarginContainer
{
    // code-defined
    public TextureButton Button;
    private Gem _gem = null;
    private static Shader _flipper = GD.Load<Shader>("res://Flip.gdshader");
    public GemUI() : base() { }
    public GemUI(Gem gem) : base()
    {
        _gem = gem;
        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        Button = new TextureButton();
        Button.Expand = true;
        Button.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        Button.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        Button.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        AddChild(Button);
        Set(gem);
    }
    public void Set(Gem gem)
    {
        _gem = gem;
        string filename;
        if (gem == null)
        {
            filename = "transparent";
        }
        else
        {
            filename = gem.Name();
        }
        switch (gem)
        {
            case Gem.Source g:
                SetDirection(g.Direction);
                break;
            case Gem.Focus g:
                SetDirection(g.Direction);
                break;
            case Gem.Stochastic g:
                Button.FlipV = !g.Orientation;
                break;
            case Gem.Mirror g:
                Button.FlipV = !g.Orientation;
                break;
        }
        Button.TextureNormal = GD.Load<Texture>($"res://texture/gem/{filename}.png");
    }
    public void Empty()
    {
        Button.TextureNormal = null;
    }
    private void SetDirection(PointInt direction)
    {
        if (direction.IntX == 0 && direction.IntY == -1)
        {
            SetDiagFlip(false);
        }
        else if (direction.IntX == 0 && direction.IntY == 1)
        {
            SetDiagFlip(false);
            Button.FlipV = true;
        }
        else if (direction.IntX == 1 && direction.IntY == 0)
        {
            SetDiagFlip(true);
            Button.FlipV = true;
        }
        else if (direction.IntX == -1 && direction.IntY == 0)
        {
            SetDiagFlip(true);
        }
        else throw new Shared.ValueError();
    }
    private void SetDiagFlip(bool do_flip)
    {
        if (do_flip)
        {
            ShaderMaterial mat = new ShaderMaterial();
            mat.Shader = _flipper;
            Material = mat;
        }
    }
}
