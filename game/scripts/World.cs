using Godot;
using System;

public class World : Node2D
{
    public TextureRect BackRect;
    public MageUI MyMageUI;
    ShaderMaterial BackShader;
    public float AspectRatio;

    public override void _Ready()
    {
        BackRect = GetNode<TextureRect>("Background");
        MyMageUI = GetNode<MageUI>("Mage");
        BackShader = (ShaderMaterial)BackRect.Material;
        AspectRatio = BackRect.RectMinSize.y / BackRect.RectMinSize.x;
        BackShader.SetShaderParam("aspect_ratio", AspectRatio);
        UpdateBack();
        MyMageUI.Resting();
    }

    private static readonly float SOFTZONE = 0;
    public override void _Process(float delta)
    {
        if (Input.IsMouseButtonPressed(((int)ButtonList.Right)))
        {
            Vector2 drag = GetLocalMousePosition();
            Vector2 v = drag.Normalized();
            float l = drag.Length() / SOFTZONE;
            if (l < 1)
                v *= l;
            GameState.Transient.LocationOffset += (
                delta * Params.SPEED * v
            );
            UpdateBack();
            MyMageUI.Walking();
        }
        else
        {
            MyMageUI.Resting();
        }
    }

    public void UpdateBack()
    {
        BackShader.SetShaderParam(
            "offset_g", GameState.Transient.LocationOffset
            - new Vector2(.5f, .5f * AspectRatio)
        );
    }
}
