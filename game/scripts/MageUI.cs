using Godot;
using System;

public class MageUI : Node2D
{
    public Node2D Walk;
    public AnimatedSprite WalkBody;
    public Sprite WalkWand;
    public Sprite WalkArms;
    public Node2D Jump;
    public AnimatedSprite JumpPrep;
    public AnimatedSprite JumpSpin;
    public override void _Ready()
    {
        Walk = GetNode<Node2D>("Walk");
        WalkBody = GetNode<AnimatedSprite>("Walk/Body");
        WalkWand = GetNode<Sprite>("Walk/Wand");
        WalkArms = GetNode<Sprite>("Walk/Arms");
        Jump = GetNode<Node2D>("Jump");
        JumpPrep = GetNode<AnimatedSprite>("Jump/Prep");
        JumpSpin = GetNode<AnimatedSprite>("Jump/Spin");
    }

    public void Resting()
    {
        Walk.Visible = true;
        Jump.Visible = false;
        WalkBody.Playing = false;
    }
    public void Walking()
    {
        Walk.Visible = true;
        Jump.Visible = false;
        WalkBody.Playing = true;
    }
    public void Preping()
    {
        Walk.Visible = false;
        Jump.Visible = true;
        JumpPrep.Visible = true;
        JumpSpin.Visible = false;
    }
    public void Spinning()
    {
        Walk.Visible = false;
        Jump.Visible = true;
        JumpPrep.Visible = false;
        JumpSpin.Visible = true;
    }
    public void Hold(Wand wand)
    {
        if (wand == null)
        {
            WalkWand.Visible = false;
        }
        WalkWand.Texture = wand.Texture();
        WalkWand.Visible = true;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
