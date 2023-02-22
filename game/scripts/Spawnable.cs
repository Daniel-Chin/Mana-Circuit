using Godot;

public interface Spawnable
{
}

public class SpawnableUI : Node2D
{
    public Spawnable MySpawnable;

    public Sprite MySprite;
    public SpawnableUI(Spawnable s)
    {
        MySpawnable = s;
        MySprite = new Sprite();
        AddChild(MySprite);
        MySprite.Scale = new Vector2(4, 4);
    }
}
