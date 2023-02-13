using Godot;
using System;

public class GemUI : Node2D
{
    private Sprite _sprite = null;
    private Gem _gem = null;
    private bool _ok = false;
    public void Set(Gem gem)
    {
        _gem = gem;
        if (_sprite != null)
            Load();
    }
    private void Load()
    {
        _sprite.Texture = GD.Load<Texture>("res://texture/source.png");
    }
    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("MySprite");
        if (_gem != null)
            Load();
    }
}
