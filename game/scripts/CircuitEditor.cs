using Godot;
using System;
using System.Text;
using System.Collections.Generic;

public class CircuitEditor : WindowDialog
{
    // code-defined
    [Signal] public delegate void finished();
    public GemListScene _gemList;
    public VBoxContainer VBox;
    public RichTextLabel HeadingLabel;
    public RichTextLabel ExplainLabel;
    public MagicItem Editee;
    public CircuitUI MyCircuitUI;
    public MarginContainer CircuitUIContainer;
    public Button UninstallButton;
    private bool _confirmingUninstallAll;
    private float _confirmingUninstallAllTime;
    public CircuitEditor() : base()
    {
        _confirmingUninstallAll = false;

        RectMinSize = new Vector2(700, 550);
        Theme = Shared.THEME;
        VBox = new VBoxContainer();
        AddChild(VBox);
        VBox.RectMinSize = RectMinSize;
        HeadingLabel = new RichTextLabel();
        ExplainLabel = new RichTextLabel();
        VBox.AddChild(HeadingLabel);
        VBox.AddChild(Pad.H(ExplainLabel, 10));
        HeadingLabel.BbcodeEnabled = true;
        HeadingLabel.FitContentHeight = true;
        ExplainLabel.BbcodeEnabled = true;
        ExplainLabel.RectMinSize = new Vector2(0, 100);
        CircuitUIContainer = new MarginContainer();
        VBox.AddChild(CircuitUIContainer);
        CircuitUIContainer.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        CenterContainer centerer = new CenterContainer();
        VBox.AddChild(Pad.V(centerer, 10));
        UninstallButton = new Button();
        centerer.AddChild(UninstallButton);
        UpdateUninstallButton();
        UninstallButton.Connect(
            "pressed", this, "UninstallAllClicked"
        );
        UninstallButton.Connect(
            "mouse_exited", this, "UninstallAllMouseExited"
        );

        _gemList = new GemListScene();
        Connect("popup_hide", this, "OnPopupHide");
    }

    public void Popup()
    {
        AddChild(_gemList);
        _gemList.Connect(
            "finished", this, "onGemListFinish"
        );
        _gemList.ListEditable();
        _gemList.PopupCentered();
    }

    public void onGemListFinish()
    {
        if (_gemList.Selected == null)
        {
            OnPopupHide();
            return;
        }
        Editee = _gemList.Selected;
        MyCircuitUI = new CircuitUI(
            Editee, 0, true, true, new Dictionary<Simplest, CircuitUI>()
        );
        MyCircuitUI.Rebuild();
        CircuitUIContainer.AddChild(MyCircuitUI);
        MyCircuitUI.Connect(
            "modified", this, "CircuitModified"
        );
        MyCircuitUI.Connect(
            "new_explain", this, "NewExplain"
        );
        CircuitModified();
        PopupCentered();
    }
    public void OnPopupHide()
    {
        QueueFree();
        EmitSignal("finished");
    }

    public void CircuitModified()
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

    public void NewExplain(string explain)
    {
        ExplainLabel.BbcodeText = explain;
    }

    int layoutWarm = 2;
    public override void _Process(float delta)
    {
        // the aspectContainer circuitUI doesn't refresh its rect when it should.
        if (layoutWarm != 0 && MyCircuitUI != null) {
            layoutWarm --;
            if (layoutWarm % 2 == 0) {
                VBox.Show();
            } else {
                VBox.Hide();
            }
        }
        if (_confirmingUninstallAll) {
            UninstallButton.Disabled = Main.MainTime < _confirmingUninstallAllTime + Params.CONFIRM_DEADZONE;
        }
    }

    public void UninstallAllClicked() {
        if (_confirmingUninstallAll) {
            Shared.Assert(Main.MainTime >= _confirmingUninstallAllTime + Params.CONFIRM_DEADZONE);
            _confirmingUninstallAll = false;
            MyCircuitUI.MyCircuit.ClearPlacables();
            CircuitModified();
            MyCircuitUI.Rebuild();
        } else {
            _confirmingUninstallAll = true;
            _confirmingUninstallAllTime = Main.MainTime;
        }
        UpdateUninstallButton();
    }
    public void UninstallAllMouseExited() {
        _confirmingUninstallAll = false;
        UpdateUninstallButton();
    }

    private void UpdateUninstallButton() {
        if (_confirmingUninstallAll) {
            UninstallButton.Text = " Click again to confirm ";
        } else {
            UninstallButton.Text = " Uninstall all gems ";
            UninstallButton.Disabled = false;
        }
    }
}
