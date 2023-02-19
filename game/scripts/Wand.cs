using System;
using Godot;

public abstract class Wand : MagicItem
{
    public Circuit MyCircuit;
    public static Type[] Types = new Type[] {
        typeof(Test),
    };
    public abstract string Name();
    public abstract string DisplayName();
    public abstract void Init();
    public Texture Texture()
    {
        return GD.Load<Texture>($"res://texture/wand/{Name()}.png");
    }
    public int TypeID(Wand wand)
    {
        for (int i = 0; i < Types.Length; i++)
        {
            if (wand.GetType() == Types[i])
                return i;
        }
        throw new Shared.ValueError();
    }

    public class Test : Wand
    {
        public override string Name()
        {
            return "test_wand";
        }
        public override string DisplayName()
        {
            return "Test Wand";
        }
        public override void Init()
        {
            Circuit c = new Circuit(new PointInt(8, 8));
            for (int i = 0; i < 8; i++)
            {
                c.Add(new Gem.Wall().Place(new PointInt(0, i)), true);
                c.Add(new Gem.Wall().Place(new PointInt(7, i)), true);
                c.Add(new Gem.Wall().Place(new PointInt(i, 0)), true);
                c.Add(new Gem.Wall().Place(new PointInt(i, 7)), true);
            }

            Gem source = new Gem.Source(new PointInt(1, 0)).Place(new PointInt(1, 4));
            c.Add(source);

            Gem drain = new Gem.Drain().Place(new PointInt(6, 4));
            c.Add(drain);

            c.Add(new Gem.WeakMult().Place(new PointInt(3, 2)));
            c.Add(new Gem.Mirror(false).Place(new PointInt(3, 1)));
            c.Add(new Gem.Mirror(true).Place(new PointInt(4, 1)));
            c.Add(new Gem.Stochastic(false).Place(new PointInt(4, 4)));
            c.Add(new Gem.Focus(new PointInt(1, 0)).Place(new PointInt(3, 4)));
            MyCircuit = c;
        }
    }
}
