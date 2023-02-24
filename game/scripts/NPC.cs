using Godot;

public abstract class NPC : Spawnable
{
    public abstract string Name();
    public Texture Texture()
    {
        return GD.Load<Texture>($"res://texture/npc/{Name()}.png");
    }

    public class GemExpert : NPC
    {
        public override string Name()
        {
            return "expert";
        }
    }

    public class Inventor : NPC
    {
        public override string Name()
        {
            return "inventor";
        }
    }
    public class Shop : NPC
    {
        public override string Name()
        {
            return "shop";
        }
    }
    public class WandSmith : NPC
    {
        public override string Name()
        {
            return "wandSmith";
        }
    }
}
