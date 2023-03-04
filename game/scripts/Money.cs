using Godot;
using System;

public class Money : Node2D, SpawnableUI
{
    // code-defined
    public RichTextLabel Label;
    public Simplest Amount;
    private static readonly float REPEL = 50000f;
    private static readonly float FRICTION = 600f;
    public Vector2 Velocity = new Vector2(0, 0);
    public Money() : base() { }
    public Money(Simplest amount) : base()
    {
        Amount = amount;
        Label = new RichTextLabel();
        AddChild(Label);
        Label.BbcodeEnabled = true;
        Label.ScrollActive = false;
        Label.RectMinSize = new Vector2(100, 50);
        Label.RectSize = Label.RectMinSize;
        Label.RectPosition = -Label.RectMinSize * .5f;
        Label.Theme = Shared.THEME;
        SetAmount();
    }
    public void Step(Vector2 force, float dt)
    {
        Velocity += REPEL * force * dt;
        Vector2 friction = -Velocity.Normalized() * FRICTION;
        Velocity += friction * dt;
        // Velocity *= (float)Math.Exp(-dt * FRICTION);
        if (Velocity.Length() >= 5)
            Position += Velocity * dt;
    }
    public void SetAmount()
    {
        string text;
        if (Amount.Equals(Simplest.One()))
        {
            text = "";
        }
        else
        {
            text = MathBB.Build(Amount);
        }
        Label.BbcodeText = $"[center][color=yellow]${text}[/color][/center]";
    }
}
