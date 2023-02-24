using Godot;
using System;
using System.Text;


public class CircuitEditor : WindowDialog
{
    // code-defined
    public GemListScene _gemList;
    public VBoxContainer VBox;
    public RichTextLabel HeadingLabel;
    public RichTextLabel ExplainLabel;
    public AspectRatioContainer Aspect;
    public MagicItem Editee;
    public CircuitUI MyCircuitUI;
    public CircuitEditor() : base()
    {
        RectMinSize = new Vector2(700, 550);
        Theme = Shared.THEME;
        VBox = new VBoxContainer();
        AddChild(VBox);
        VBox.RectMinSize = RectMinSize;
        HeadingLabel = new RichTextLabel();
        ExplainLabel = new RichTextLabel();
        Aspect = new AspectRatioContainer();
        VBox.AddChild(HeadingLabel);
        VBox.AddChild(ExplainLabel);
        VBox.AddChild(Aspect);
        HeadingLabel.BbcodeEnabled = true;
        HeadingLabel.FitContentHeight = true;
        ExplainLabel.BbcodeEnabled = true;
        ExplainLabel.RectMinSize = new Vector2(0, 100);
        Aspect.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        Aspect.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;
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
            sB.Append(": f(x) = ");
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
