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
        Visible = false;
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
        bool inYellow = false;
        Label.Clear();
        foreach (var part in message.Split('*'))
        {
            Label.AppendBbcode(part);
            if (inYellow)
            {
                Label.Pop();
            }
            else
            {
                Label.PushColor(Colors.Yellow);
            }
            inYellow = !inYellow;
        }
        if (inYellow)
            Label.Pop();
        Label.PercentVisible = 0;
        _timeElasped = 0;
        Visible = true;
    }

    public override void _Process(float delta)
    {
        _timeElasped += delta;
        Label.PercentVisible = Math.Min(
            1f, Params.TEXT_ROLL_SPEED * _timeElasped / Label.Text.Length
        );
    }

    public void SkipRoll()
    {
        _timeElasped = Label.Text.Length / Params.TEXT_ROLL_SPEED;
    }

    public void _on_Mask_pressed()
    {
        if (_timeElasped < Label.Text.Length / Params.TEXT_ROLL_SPEED)
        {
            SkipRoll();
            return;
        }
        Visible = false;
        Director.OnEventStepCompelte();
    }
}
