using Godot;
using System;

public class PocketScene : Node2D
{
    public WindowDialog MyDialog;
    public override void _Ready()
    {
        MyDialog = GetNode<WindowDialog>("MyDialog");
    }
}
