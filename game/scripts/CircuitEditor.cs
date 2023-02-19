using Godot;
using System;
using System.Diagnostics;

public class CircuitEditor : WindowDialog
{
    public GemListScene _gemList;
    public VBoxContainer VBox;
    public RichTextLabel LFSLabel;
    public RichTextLabel ExplainLabel;
    public CenterContainer Center;
    public CircuitUI MyCircuitUI;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        LFSLabel = GetNode<RichTextLabel>("VBox/LFS");
        ExplainLabel = GetNode<RichTextLabel>("VBox/Explain");
        Center = GetNode<CenterContainer>("VBox/Center");
        _gemList = new GemListScene();
    }

    public void Popup()
    {
        AddChild(_gemList);
        _gemList.Connect(
            "itemSelected", this, "onGemListSelect"
        );
        _gemList.ListEditables();
        _gemList.PopupCentered();
    }

    public void onGemListSelect()
    {
        MyCircuitUI = new CircuitUI(_gemList.Selected, 0);
        Center.AddChild(MyCircuitUI);
        PopupCentered();
    }
}
