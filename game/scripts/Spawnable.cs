using System;
using Godot;

public interface SpawnableSpecial
{
    // Enemy, NPC, or DropedItem
}

public interface SpawnableUI
{
    // money, attack
}

public class SpawnableSpecialUI : Node2D, SpawnableUI
{
    public SpawnableSpecial MySpawnable;
    public Sprite MySprite;
    public bool Exposed;
    public SpawnableSpecialUI(SpawnableSpecial s)
    {
        MySpawnable = s;
        MySprite = new Sprite();
        AddChild(MySprite);
        MySprite.Scale = Params.SPRITE_SCALE;
        Exposed = false;
    }
    public void Moved(Vector2 viewRadius) {
        if (Exposed) return;
        if (Math.Abs(Position.x) > viewRadius.x) return;
        if (Math.Abs(Position.y) > viewRadius.y) return;
        Exposed = true;
    }
}
