using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class World : Node2D
{
    public TextureRect BackRect;
    public MageUI MyMageUI;
    ShaderMaterial BackShader;
    public float AspectRatio;
    public List<Money> Moneys;

    public override void _Ready()
    {
        BackRect = GetNode<TextureRect>("Background");
        MyMageUI = GetNode<MageUI>("MageUI");
        BackShader = (ShaderMaterial)BackRect.Material;
        AspectRatio = BackRect.RectMinSize.y / BackRect.RectMinSize.x;
        BackShader.SetShaderParam("aspect_ratio", AspectRatio);
        UpdateBack();
        MyMageUI.Resting();
        MyMageUI.Hold(GameState.Persistent.MyWand);
        Moneys = new List<Money>();
    }

    private static readonly float SOFTZONE = 0;
    public override void _Process(float delta)
    {
        if (GameState.Transient.NPCPausedWorld)
            return;
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
        UpdateMoneys(delta);
    }

    private void UpdateMoneys(float dt)
    {
        for (int i = 0; i < Moneys.Count; i++)
        {
            Money m0 = Moneys[i];
            foreach (Money m1 in Moneys.Skip(i + 1))
            {
                Vector2 displace = m0.Position - m1.Position;
                Vector2 force = displace.Normalized() / displace.Length();
                m0.Step(force, dt);
                m1.Step(-force, dt);
            }
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
