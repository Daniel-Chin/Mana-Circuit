using Godot;
using System;

public class CircuitEditor : WindowDialog
{
    public RichTextLabel LFSLabel;
    public RichTextLabel ExplainLabel;
    public CircuitUI MyCircuitUI;
    public override void _Ready()
    {
        LFSLabel = GetNode<RichTextLabel>("VBox/LFS");
        ExplainLabel = GetNode<RichTextLabel>("VBox/Explain");
        // MyCircuitUI = new CircuitUI(0);
        AddChild(MyCircuitUI);
    }
}
