using Godot;
using System;

public class ContainerButton : MarginContainer
{
    // code-defined
    public Button MyButton;
    private Control _child;
    private RichTextLabel _label;
    public bool Confirming;
    private float _confirmingTime;
    public ContainerButton()
    {
        // This is for the Godot editor, I guess?
        // Do nothing
    }
    public ContainerButton(Control child)
    {
        Confirming = false;
        _child = child;
        MyButton = new Button();
        AddChild(MyButton);
        AddChild(_child);
        Shared.PropogateMouseFilter(_child, MouseFilterEnum.Ignore);
        MyButton.Connect(
            "mouse_exited", this, "MouseExited"
        );
    }

    public RichTextLabel TextInstead() {
        if (Confirming) {
            FreeText();
        }
        Confirming = true;
        _child.Hide();
        _label = new RichTextLabel();
        AddChild(_label);
        _label.BbcodeEnabled = true;
        _label.SizeFlagsVertical = (int)Container.SizeFlags.ShrinkCenter;
        _label.FitContentHeight = true;
        _label.MouseFilter = MouseFilterEnum.Ignore;
        _confirmingTime = Main.MainTime;
        return _label;
    }

    public void MouseExited() {
        if (Confirming) {
            FreeText();
        }
    }
    public void FreeText() {
        Confirming = false;
        _label.QueueFree();
        _child.Show();
    }

    public override void _Process(float delta) {
        if (Confirming) {
            MyButton.Disabled = Main.MainTime < _confirmingTime + Params.CONFIRM_DEADZONE;
        } else {
            MyButton.Disabled = false;
        }
    }
}
