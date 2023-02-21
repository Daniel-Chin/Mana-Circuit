using System;
using System.Collections.Generic;
using Godot;

namespace GameState
{
    public static class Persistent
    {
        public static Simplest Money;
        public static (double, Simplest) Location;
        // polar
        public static Wand MyWand;
        public static Dictionary<string, int> HasGems;
        public static Dictionary<int, (int, CustomGem)> HasCustomGems;
        public static CustomGem MyTypelessGem;
        public static int JumperStage;
        public static void Init()
        {
            Money = Simplest.Zero();
            Location = (0, Simplest.Zero());
            HasGems = new Dictionary<string, int>();
            HasCustomGems = new Dictionary<int, (int, CustomGem)>();
            MyTypelessGem = null;
            JumperStage = 0;

            DebugInit();

            Ready();
        }

        public static void DebugInit()
        {
            MyWand = new Wand.Test();
            MyWand.Init();
            HasGems.Add("addOne", 1);
            HasGems.Add("weakMult", 9);
            HasGems.Add("focus", 99);
            HasGems.Add("mirror", 99);
            HasGems.Add("stochastic", 99);
            CustomGem cG = new CustomGem(new Simplest(Rank.FINITE, 0));
            HasCustomGems.Add(0, (2, cG));
            HasCustomGems.Add(1, (2, new CustomGem(new Simplest(Rank.FINITE, 1))));
            HasCustomGems.Add(3, (2, new CustomGem(new Simplest(Rank.FINITE, 3))));
            HasCustomGems.Add(6, (2, new CustomGem(new Simplest(Rank.FINITE, 6))));
            MyTypelessGem = new CustomGem(Simplest.W());

            Circuit c = new Circuit(new PointInt(8, 8));
            for (int i = 0; i < 8; i++)
            {
                c.Add(new Gem.Wall(), new PointInt(0, i), true);
                c.Add(new Gem.Wall(), new PointInt(7, i), true);
                c.Add(new Gem.Wall(), new PointInt(i, 0), true);
                c.Add(new Gem.Wall(), new PointInt(i, 7), true);
            }

            Gem source = new Gem.Source(new PointInt(1, 0));
            c.Add(source, new PointInt(1, 4));

            Gem drain = new Gem.Drain();
            c.Add(drain, new PointInt(6, 4));

            c.Add(new Gem.WeakMult(), new PointInt(3, 2));
            c.Add(new Gem.WeakMult(), new PointInt(4, 2));
            c.Add(new Gem.WeakMult(), new PointInt(4, 3));
            c.Add(new Gem.Mirror(false), new PointInt(3, 1));
            c.Add(new Gem.Mirror(true), new PointInt(4, 1));
            c.Add(new Gem.Stochastic(false), new PointInt(4, 4));
            c.Add(new Gem.Focus(new PointInt(1, 0)), new PointInt(3, 4));
            cG.MyCircuit = c;
        }
        public static void WriteDisk() { }
        public static void LoadDisk()
        {
            Ready();
        }
        public static void Ready()
        {
            foreach (var item in HasCustomGems)
            {
                item.Value.Item2.Eval();
            }
            if (MyTypelessGem != null)
            {
                MyTypelessGem.Eval();
            }

        }
    }
    public static class Transient
    {
        public static Vector2 LocationOffset;
        public static void Init()
        {
            LocationOffset = new Vector2(0, 0);
        }
    }
}
