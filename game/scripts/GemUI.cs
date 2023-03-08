using Godot;
using System;
using System.Collections.Generic;

public class GemUI : AspectRatioContainer
{
    // code-defined
    [Signal] public delegate void mouse_entered_overlay();
    [Signal] public delegate void mouse_exited_overlay();
    public ColorRect Tinter;
    public TextureButton Button;
    public CircuitUI MyCircuitUI;
    private Gem _gem = null;
    public int RecursionDepth;
    public bool IsInCG;
    public bool SimParticles;
    private static Shader _flipper = GD.Load<Shader>("res://Flip.gdshader");
    public GemUI() : base() { }
    public GemUI(
        Gem gem, int recursionDepth, bool isInCG, bool simParticles
    ) : base()
    {
        _gem = gem;
        RecursionDepth = recursionDepth;
        IsInCG = isInCG;
        SimParticles = simParticles;
        Ratio = 1f;
        SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
        Button = new TextureButton();
        Button.Expand = true;
        Button.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        AddChild(Button);
        if (recursionDepth == 0)
        {
            Tinter = new ColorRect();
            Tinter.Color = Colors.Transparent;
            AddChild(Tinter);
            MoveChild(Tinter, 0);
        }
    }

    public void PaintIn()
    {
        string filename;
        if (_gem is CustomGem cG) {
            if (cG.MetaLevel.MyRank != Rank.FINITE) {
                return;
            }
            MyCircuitUI = new CircuitUI(
                cG, RecursionDepth + 1, false, SimParticles
            );
            filename = "transparent";
            AddChild(MyCircuitUI);
            MoveChild(MyCircuitUI, 0);
            MyCircuitUI.Name += "_CG_CircuitUI";
            MyCircuitUI.Rebuild();
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

    public void ConnectMouseOver()
    {
        Button.Connect(
            "mouse_entered", this, "MouseEntered"
        );
        Button.Connect(
            "mouse_exited", this, "MouseExited"
        );
    }

    public void MouseEntered()
    {
        Tinter.Color = Color.FromHsv(0, 0, 1, .2f);
        EmitSignal("mouse_entered_overlay");
    }

    public void MouseExited()
    {
        Tinter.Color = Colors.Transparent;
        EmitSignal("mouse_exited_overlay");
    }

    public override void _Process(float delta)
    {
        if (
            _gem is CustomGem cG 
            && cG.MetaLevel.MyRank != Rank.FINITE
        ) {
            Button.TextureNormal = ???;
        }
    }
}
