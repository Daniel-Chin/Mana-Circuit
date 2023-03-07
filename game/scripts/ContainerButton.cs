using Godot;
using System;

public class ContainerButton : MarginContainer
{
    // code-defined
    public Button MyButton;
    public ContainerButton()
    {
        // This is for the Godot editor, I guess?
        // Do nothing
    }
    public ContainerButton(Control child)
    {
        MyButton = new Button();
        AddChild(MyButton);
        AddChild(child);
        Shared.PropogateMouseFilter(child, MouseFilterEnum.Ignore);
    }
}
