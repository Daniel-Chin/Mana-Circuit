using Godot;
using System;

public class EnemyUI : Node2D
{
    public RichTextLabel label;
    public override void _Ready()
    {
        label = GetNode<RichTextLabel>("RichTextLabel");
    }

    public void SetHP(Simplest simplest)
    {
        label.BbcodeText = $"[center]{MathBB.Build(simplest)}[/center]";
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
