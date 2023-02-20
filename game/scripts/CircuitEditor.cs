using Godot;
using System;
using System.Diagnostics;

public class CircuitEditor : WindowDialog
{
    public GemListScene _gemList;
    public VBoxContainer VBox;
    public RichTextLabel LFSLabel;
    public RichTextLabel ExplainLabel;
    public AspectRatioContainer Aspect;
    public CircuitUI MyCircuitUI;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        LFSLabel = GetNode<RichTextLabel>("VBox/LFS");
        ExplainLabel = GetNode<RichTextLabel>("VBox/Explain");
        Aspect = GetNode<AspectRatioContainer>("VBox/Aspect");
        _gemList = new GemListScene();
        Connect("popup_hide", this, "OnPopupHide");
    }

    public void Popup()
    {
        AddChild(_gemList);
        _gemList.Connect(
            "finished", this, "onGemListFinish"
        );
        _gemList.ListEditables();
        _gemList.PopupCentered();
    }

    public void onGemListFinish()
    {
        if (_gemList.Selected == null)
        {
            QueueFree();
            return;
        }
        MyCircuitUI = new CircuitUI(_gemList.Selected, 0);
        Aspect.AddChild(MyCircuitUI);
        PopupCentered();
    }
    public void OnPopupHide()
    {
        QueueFree();
    }
}
