using Godot;
using System;

public class LowPanel : PanelContainer
{
    public TextureRect Face;
    public RichTextLabel Label;
    public HBoxContainer ButtonsHBox;
    public Button Button0;
    public Button Button1;
    public TextureButton Mask;
    private float _timeElasped;
    private bool _hasButtons;
    public override void _Ready()
    {
        _hasButtons = false;

        Face = GetNode<TextureRect>("HBox/Face");
        Label = GetNode<RichTextLabel>("HBox/VBox/Label");
        ButtonsHBox = GetNode<HBoxContainer>("HBox/VBox/Buttons");
        Button0 = GetNode<Button>("HBox/VBox/Buttons/Centerer0/Button0");
        Button1 = GetNode<Button>("HBox/VBox/Buttons/Centerer1/Button1");
        Mask = GetNode<TextureButton>("Mask");
        ButtonsHBox.Visible = false;

        Button0.Connect(
            "pressed", this, "Button0Clicked"
        );
        Button1.Connect(
            "pressed", this, "Button1Clicked"
        );
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
        bool inCyan = false;
        Label.Clear();
        foreach (var part in message.Split('*'))
        {
            Label.AppendBbcode(part);
            if (inCyan)
            {
                Label.Pop();
            }
            else
            {
                Label.PushColor(Colors.Cyan);
            }
            inCyan = !inCyan;
        }
        if (inCyan)
            Label.Pop();
        Label.PercentVisible = 0;
        _timeElasped = 0;
        Main.Singleton.VBoxLowPanel.Visible = true;
    }

    public void SetButtons(string text0, string text1)
    {
        _hasButtons = true;
        Button0.Text = $" {text0} ";
        Button1.Text = $" {text1} ";
    }
    public void NoButtons()
    {
        _hasButtons = false;
        ButtonsHBox.Visible = false;
        Mask.Visible = true;
    }

    public override void _Process(float delta)
    {
        _timeElasped += delta;
        float rollProgress = Params.TEXT_ROLL_SPEED * _timeElasped / Label.Text.Length;
        if (rollProgress >= 1) {
            Label.PercentVisible = 1;
            if (_hasButtons) {
                ButtonsHBox.Visible = true;
                Mask.Visible = false;
            }
        } else {
            Label.PercentVisible = rollProgress;
        }
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
        if (_hasButtons)
            return;
        Main.Singleton.VBoxLowPanel.Visible = false;
        Director.OnEventStepComplete();
    }

    public void Button0Clicked() {
        Director.NowEvent.ButtonClicked(0);
        Main.Singleton.VBoxLowPanel.Visible = false;
    }
    public void Button1Clicked() {
        Director.NowEvent.ButtonClicked(1);
        Main.Singleton.VBoxLowPanel.Visible = false;
    }
}
