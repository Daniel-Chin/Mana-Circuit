using Godot;
using System;

public class GemUI : AspectRatioContainer
{
    // code-defined
    public TextureButton Button;
    public CircuitUI MyCircuitUI;
    private Gem _gem = null;
    public int RecursionDepth;
    public bool IsInCG;
    private static Shader _flipper = GD.Load<Shader>("res://Flip.gdshader");
    public GemUI() : base() { }
    public GemUI(Gem gem, int recursionDepth, bool isInCG) : base()
    {
        _gem = gem;
        RecursionDepth = recursionDepth;
        IsInCG = isInCG;
        Ratio = 1f;
        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        Button = new TextureButton();
        Button.Expand = true;
        Button.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        Button.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        Button.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        AddChild(Button);
        Set();
    }
    private void Set()
    {
        string filename;
        if (_gem is CustomGem cG)
        {
            filename = "transparent";
            MyCircuitUI = new CircuitUI(
                new UnionWandGem(cG), RecursionDepth + 1
            );
            AddChild(MyCircuitUI);
            MoveChild(MyCircuitUI, 0);
            MyCircuitUI.Name += "_CG_CircuitUI";
        }
        else
        {
            if (_gem == null)
            {
                filename = "transparent";
            }
            else
            {
                filename = _gem.Name();
            }
            switch (_gem)
            {
                case Gem.Source g:
                    if (IsInCG)
                    {
                        filename = "arrow";
                    }
                    SetDirection(g.Direction);
                    break;
                case Gem.Drain g:
                    if (IsInCG)
                    {
                        filename = "arrow";
                        SetDirection(new PointInt(1, 0));
                    }
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
            Button.Material = mat;
        }
    }
}
