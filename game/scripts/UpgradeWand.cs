using Godot;
using System;

public class UpgradeWand : WindowDialog
{
    // code-defined
    [Signal] public delegate void finished();
    Wand NewWand;
    public UpgradeWand() : base()
    {
        NewWand = GameState.Persistent.MyWand.UpgradeInto();
        Connect("popup_hide", this, "OnPopupHide");

        RectMinSize = new Vector2(900, 500);
        Theme = Shared.THEME;
        VBoxContainer vBox = new VBoxContainer();
        AddChild(vBox);
        vBox.RectMinSize = RectMinSize;
        HBoxContainer hBox = new HBoxContainer();
        vBox.AddChild(hBox);
        hBox.Alignment = HBoxContainer.AlignMode.Center;
        hBox.SizeFlagsVertical = (int)Container.SizeFlags.ExpandFill;

        AddWand(GameState.Persistent.MyWand, hBox);
        Label arrow = new Label();
        arrow.Text = "->";
        arrow.AddFontOverride("font", Shared.NewFont(100));
        AddWand(NewWand, hBox);

        MarginContainer margin = new MarginContainer();
        vBox.AddChild(margin);
        margin.RectMinSize = new Vector2(0, 80);
        margin.AddConstantOverride("margin_left", 20);
        margin.AddConstantOverride("margin_right", 20);
        margin.AddConstantOverride("margin_top", 20);
        margin.AddConstantOverride("margin_bottom", 20);
        hBox = new HBoxContainer();
        margin.AddChild(hBox);
        hBox.Alignment = HBoxContainer.AlignMode.End;

        Button buy = new Button();
        hBox.AddChild(buy);
        buy.Text = "  Upgrade!  ";
        buy.Disabled = !(GameState.Persistent.Money >= PriceOf(NewWand));
        buy.Connect("pressed", this, "Buy");

        RichTextLabel priceLabel = new RichTextLabel();
        hBox.AddChild(priceLabel);
        priceLabel.BbcodeEnabled = true;
        priceLabel.BbcodeText = $"[color=yellow]${MathBB.Build(PriceOf(NewWand))}[/color]";
        priceLabel.RectMinSize = new Vector2(150, 0);

        Button leave = new Button();
        hBox.AddChild(leave);
        leave.Text = " Leave ";
        leave.Connect("pressed", this, "Leave");
    }

    private void AddWand(Wand wand, Control node)
    {
        VBoxContainer vBox = new VBoxContainer();
        node.AddChild(vBox);
        vBox.SizeFlagsHorizontal = (int)Container.SizeFlags.ExpandFill;
        Label title = new Label();
        vBox.AddChild(title);
        title.Text = wand.DisplayName();
        title.Align = Label.AlignEnum.Center;
        vBox.AddChild(new CircuitUI(wand, 0, false, true));
    }

    public void Buy()
    {
        GameState.Persistent.Money = Simplest.Eval(
            GameState.Persistent.Money, Operator.MINUS,
            PriceOf(NewWand)
        );
        GameState.Persistent.MyWand = NewWand;
        Main.Singleton.WandReplaced();
        Hide();
    }

    public void Leave()
    {
        EmitSignal("finished");
        QueueFree();
    }
    public void OnPopupHide()
    {
        Leave();
    }

    private Simplest PriceOf(Wand wand)
    {
        switch (wand)
        {
            case Wand.Guitar _:
                return new Simplest(Rank.FINITE, 13);
            default:
                throw new Shared.TypeError();
        }
    }
}
