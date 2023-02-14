using Godot;
using System;

public class PocketScene : Node2D
{
    [Signal] public delegate void gemClicked(int gem_i);
    public WindowDialog MyDialog;
    public VBoxContainer MyVBox;
    // SignalAttribute
    public override void _Ready()
    {
        MyDialog = GetNode<WindowDialog>("MyDialog");
        MyVBox = GetNode<VBoxContainer>("MyDialog/MyScroll/MyVBox");
        foreach (Node x in MyVBox.GetChildren())
        {
            x.QueueFree();
        }
        for (int i = 0; i < Gem.N_IDS; i++)
        {
            Gem gem = Gem.FromID(i);
            GemUI gemUI = GemUI.ThisScene.Instance<GemUI>();
            MyVBox.AddChild(gemUI);
            gemUI.Set(gem);
            gemUI.Connect(
                "pressed", this, "OnClickGem",
                new Godot.Collections.Array() { i }
            );
            gemUI.RectMinSize = new Vector2(80, 80);
        }
    }

    public void OnClickGem(int gemId)
    {
        MyDialog.Visible = false;
        EmitSignal("gemClicked", gemId);
    }
}
