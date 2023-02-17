using Godot;
using System;

public class GemUI : TextureButton
{
    public static PackedScene ThisScene = GD.Load<PackedScene>("res://GemUI.tscn");
    private Gem _gem = null;
    private static Shader _flipper = GD.Load<Shader>("res://Flip.gdshader");
    public override void _Ready()
    {
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
                FlipV = !g.Orientation;
                break;
            case Gem.Mirror g:
                FlipV = !g.Orientation;
                break;
            case null:
                StretchMode = StretchModeEnum.Tile;
                break;
        }
        TextureNormal = GD.Load<Texture>($"res://texture/gem/{filename}.png");
    }
    public void Empty()
    {
        TextureNormal = null;
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
            FlipV = true;
        }
        else if (direction.IntX == 1 && direction.IntY == 0)
        {
            SetDiagFlip(true);
            FlipV = true;
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
