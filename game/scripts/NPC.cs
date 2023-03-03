using Godot;

public abstract class NPC : SpawnableSpecial
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
        public static Simplest PriceOf(Gem gem)
        {
            int k = (int)GemListScene.CountGemsOwned(gem).K;
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
        public void Enter() {
            Director.StartEvent(new MagicEvent.Shopping(this));
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
