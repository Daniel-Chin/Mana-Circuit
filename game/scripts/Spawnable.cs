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
    public SpawnableSpecialUI(SpawnableSpecial s)
    {
        MySpawnable = s;
        MySprite = new Sprite();
        AddChild(MySprite);
        MySprite.Scale = new Vector2(4, 4);
    }
}
