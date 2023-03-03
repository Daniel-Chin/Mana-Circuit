using Godot;

public abstract class NPC : Godot.Object, SpawnableSpecial
{
    public abstract string Name();
    public abstract void Collided();
    public Texture Texture()
    {
        return GD.Load<Texture>($"res://texture/npc/{Name()}.png");
    }

    public class GemExpert : NPC
    {
        public GemExpert() : base() {
            GameState.Persistent.Loneliness_GemExpert = 0;
            GameState.Persistent.Loneliness_Shop ++;
            GameState.Persistent.Loneliness_WandSmith ++;
        }
        public override string Name()
        {
            return "expert";
        }
        public override void Collided() {
        }
    }

    public class Shop : NPC
    {
        public Shop() : base() {
            GameState.Persistent.Loneliness_Shop = 0;
            GameState.Persistent.Loneliness_WandSmith ++;
            GameState.Persistent.Loneliness_GemExpert ++;
        }
        public override string Name()
        {
            return "shop";
        }
        public static Simplest PriceOf(Gem gem)
        {
            return PriceOf(gem, (int)GameState.Persistent.CountGemsOwned(gem).K);
        }
        public static Simplest PriceOf(Gem gem, int owned) {
            int k = owned;
            switch (gem)
            {
                case Gem.AddOne _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 4);
                    if (k == 1)
                        return new Simplest(Rank.FINITE, 9);
                    return new Simplest(Rank.FINITE, 10 * k);
                case Gem.WeakMult _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 6);
                    if (k == 1)
                        return new Simplest(Rank.FINITE, 9);
                    return new Simplest(Rank.FINITE, 5 * k);
                case Gem.Focus _:
                    return new Simplest(Rank.FINITE, 10 + k);
                case Gem.Stochastic _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 20);
                    if (k == 1)
                        return new Simplest(Rank.W_TO_THE_K, 1);
                    return new Simplest(Rank.W_TO_THE_K, k);
                case Gem.Mirror _:
                    return new Simplest(Rank.FINITE, 7 + k);
                case CustomGem cG:
                    if (cG.MetaLevel.MyRank != Rank.FINITE)
                        return Simplest.Zero();
                    return new Simplest(Rank.FINITE, 9 * (k + 1) * (cG.MetaLevel.K + 1));
                default:
                    throw new Shared.TypeError();
            }
        }
        public override void Collided() {
            Director.StartEvent(new MagicEvent.Shopping(this));
        }
    }
    public class WandSmith : NPC
    {
        public WandSmith() : base() {
            GameState.Persistent.Loneliness_WandSmith = 0;
            GameState.Persistent.Loneliness_GemExpert ++;
            GameState.Persistent.Loneliness_Shop ++;
        }
        public override string Name()
        {
            return "wandSmith";
        }
        public override void Collided() {
            CircuitEditor circuitEditor = new CircuitEditor();
            Main.Singleton.AddChild(circuitEditor);
            circuitEditor.Popup();
            Director.PauseWorld();
            circuitEditor.Connect(
                "finished", this, "Bye"
            );
        }
        public void Bye() {
            SaveLoad.Save();
            Director.UnpauseWorld();
            Main.Singleton.MySidePanel.MyCircuitUI.Rebuild();
        }
    }

    public class Inventor : NPC
    {
        public override string Name()
        {
            return "inventor";
        }
        public override void Collided() {
            Director.StartEvent(new MagicEvent.Jumping(this));
        }
    }
}
