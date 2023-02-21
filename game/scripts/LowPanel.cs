using Godot;
using System;

public class LowPanel : PanelContainer
{
    TextureRect Face;
    RichTextLabel Label;
    private float _timeElasped;
    public override void _Ready()
    {
        Face = GetNode<TextureRect>("HBox/Face");
        Label = GetNode<RichTextLabel>("HBox/Label");
    }

    public void SetFace(NPC npc)
    {
        if (npc == null)
        {
            Face.Visible = false;
            return;
        }
        Face.Texture = npc.Texture();
        Face.Visible = true;
    }

    public void Display(string message)
    {
        Label.BbcodeText = message;
        Label.PercentVisible = 0;
        _timeElasped = 0;
    }

    private static readonly float ROLL_SPEED = .3f;
    public override void _Process(float delta)
    {
        _timeElasped += delta;
        Label.PercentVisible = Math.Min(
            1f, ROLL_SPEED * _timeElasped / Label.Text.Length
        );
    }

    public void SkipRoll()
    {
        _timeElasped = 1 / ROLL_SPEED;
    }
}
