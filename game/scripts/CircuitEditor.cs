using Godot;
using System;
using System.Text;
using System.Diagnostics;

public class CircuitEditor : WindowDialog
{
    public GemListScene _gemList;
    public VBoxContainer VBox;
    public RichTextLabel HeadingLabel;
    public RichTextLabel ExplainLabel;
    public AspectRatioContainer Aspect;
    public MagicItem Editee;
    public CircuitUI MyCircuitUI;
    public override void _Ready()
    {
        VBox = GetNode<VBoxContainer>("VBox");
        HeadingLabel = GetNode<RichTextLabel>("VBox/Heading");
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
        Editee = _gemList.Selected;
        MyCircuitUI = new CircuitUI(Editee, 0);
        Aspect.AddChild(MyCircuitUI);
        MyCircuitUI.Connect(
            "modified", this, "circuitModified"
        );
        circuitModified();
        PopupCentered();
    }
    public void OnPopupHide()
    {
        QueueFree();
    }

    public void circuitModified()
    {
        StringBuilder sB = new StringBuilder();
        sB.Append("[center]");
        sB.Append(Editee.DisplayName());
        if (Editee is CustomGem cG)
        {
            cG.Eval();
            sB.Append(" (x) = ");
            sB.Append(MathBB.Build(cG.CachedMultiplier));
            sB.Append(" x");
            if (cG.CachedAdder != null)
            {
                sB.Append(" + ");
                sB.Append(MathBB.Build(cG.CachedAdder));
            }
        }
        sB.Append("[/center]");
        HeadingLabel.BbcodeText = sB.ToString();
    }
}
