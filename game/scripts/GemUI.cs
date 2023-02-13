using Godot;
using System;

public class GemUI : TextureRect
{
    private Gem _gem = null;
    private static Shader _flipper = GD.Load<Shader>("res://Flip.gdshader");
    public override void _Ready()
    {
    }
    public void Set(Gem gem)
    {
        _gem = gem;
        if (gem == null)
        {
            Texture = null;
            return;
        }
        string filename;
        switch (gem)
        {
            case Gem.Wall g:
                filename = "wall";
                break;
            case Gem.Source g:
                filename = "source";
                SetDirection(g.Direction);
                break;
            case Gem.Drain g:
                filename = "drain";
                break;
            case Gem.AddOne g:
                filename = "addOne";
                break;
            case Gem.WeakMult g:
                filename = "weakMult";
                break;
            case Gem.Focus g:
                filename = "focus";
                SetDirection(g.Direction);
                break;
            case Gem.Stochastic g:
                filename = "stochastic";
                SetFlip(!g.Orientation);
                break;
            case Gem.Mirror g:
                filename = "mirror";
                SetFlip(!g.Orientation);
                break;
            default:
                throw new Shared.ValueError();
        }
        Texture = GD.Load<Texture>($"res://texture/{filename}.png");
    }
    private void SetDirection(PointInt direction)
    {
        if (direction.IntX == 0 && direction.IntY == -1)
        {
            SetFlip(false);
        }
        else if (direction.IntX == 0 && direction.IntY == 1)
        {
            SetFlip(false);
            FlipV = true;
        }
        else if (direction.IntX == 1 && direction.IntY == 0)
        {
            SetFlip(true);
            FlipH = true;
        }
        else if (direction.IntX == -1 && direction.IntY == 0)
        {
            SetFlip(true);
        }
        else throw new Shared.ValueError();
    }
    private void SetFlip(bool do_flip)
    {
        if (do_flip)
        {
            ShaderMaterial mat = new ShaderMaterial();
            mat.Shader = _flipper;
            Material = mat;
        }
    }
}
