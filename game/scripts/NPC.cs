using System;
using Godot;

public abstract class NPC : Godot.Object, SpawnableSpecial
{
    public abstract string Name();
    public abstract void Collided(NPCUI npcUI);
    public Texture Texture()
    {
        return GD.Load<Texture>($"res://texture/npc/{Name()}.png");
    }
    public override string ToString() {
        return Name();
    }

    public class Shop : NPC
    {
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
                        return new Simplest(Rank.FINITE, 2);
                    if (k == 1)
                        return new Simplest(Rank.FINITE, 11);
                    return new Simplest(Rank.FINITE, 10 * k);
                case Gem.WeakMult _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 6);
                    if (k == 1)
                        return new Simplest(Rank.FINITE, 9);
                    return new Simplest(Rank.FINITE, 5 * k);
                case Gem.StrongMult _:
                    return new Simplest(Rank.FINITE, 100 * (k + 1));
                case Gem.Focus _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 10);
                    // if (k == 1)
                    //     return new Simplest(Rank.W_TO_THE_K, 1);
                    return new Simplest(Rank.W_TO_THE_K, k);
                case Gem.Stochastic _:
                    if (k == 0)
                        return new Simplest(Rank.FINITE, 60);
                    return new Simplest(Rank.FINITE, 60 + k * 10);
                case Gem.Mirror _:
                    return new Simplest(Rank.FINITE, 7);
                case CustomGem cG:
                    if (cG.MetaLevel.MyRank != Rank.FINITE)
                        return Simplest.W();
                    return new Simplest(Rank.FINITE, Math.Round(
                        80 * Math.Exp(k * 6 + cG.MetaLevel.K)
                    ));
                default:
                    throw new Shared.TypeError();
            }
        }
        public override void Collided(NPCUI npcUI) {
            Director.StartEvent(new MagicEvent.Shopping(this));
        }
    }
    public class WandSmith : NPC
    {
        public override string Name()
        {
            return "wandSmith";
        }
        public override void Collided(NPCUI npcUI) {
            Director.StartEvent(new MagicEvent.Smithing(this));
        }
    }

    public class Inventor : NPC
    {
        public override string Name()
        {
            return "inventor";
        }
        public override void Collided(NPCUI npcUI) {
            if (Director.NowEvent is MagicEvent.Jumping e) {
                e.CollideAgain();
            } else {
                Director.StartEvent(new MagicEvent.Jumping(npcUI));
            }
        }
    }

    public class GemExpert : NPC
    {
        public override string Name()
        {
            return "expert";
        }
        public override void Collided(NPCUI npcUI) {
            if (Director.NowEvent is MagicEvent.Experting e) {
                ;
            } else {
                Director.StartEvent(new MagicEvent.Experting(npcUI));
            }
        }
    }
}
