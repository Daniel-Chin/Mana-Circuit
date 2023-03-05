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
        if (Exposed) {
            switch (MySpawnable) {
                case NPC.WandSmith _:
                    if (GameState.Transient.NextSpawn is NPC.WandSmith) {
                        GameState.Transient.NextSpawn = null;
                    }
                    break;
            }
            return;
        }
        if (Math.Abs(Position.x) > viewRadius.x) return;
        if (Math.Abs(Position.y) > viewRadius.y) return;

        Exposed = true;

        switch (MySpawnable) {
            case NPC.Shop _:
                GameState.Persistent.Loneliness_Shop = 0;
                break;
            case NPC.WandSmith _:
                GameState.Persistent.Loneliness_WandSmith = 0;
                if (GameState.Transient.NextSpawn is NPC.WandSmith) {
                    GameState.Transient.NextSpawn = null;
                }
                break;
            case NPC.GemExpert _:
                GameState.Persistent.Loneliness_GemExpert = 0;
                break;
            default:
                break;
        }
    }
}
