using System;
using System.IO;

using Godot;

public abstract class Wand : MagicItem, JSONable
{
    public Circuit MyCircuit { get; set; }

    public static Type[] Types = new Type[] {
        typeof(Test),
    };
    public abstract void Init();
    public Texture Texture()
    {
        return GD.Load<Texture>($"res://texture/wand/{Name()}.png");
    }
    public Texture TextureFlat()
    {
        return GD.Load<Texture>($"res://texture/wand/{Name()}_flat.png");
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
    public abstract Wand UpgradeInto();
    public abstract Simplest Price();

    public void ToJSON(StreamWriter writer)
    {
        writer.WriteLine("[");
        switch (this)
        {
            case Test w:
                JSON.Store("test", writer);
                break;
            case Staff w:
                JSON.Store("staff", writer);
                break;
            default:
                throw new Shared.ValueError();
        }
        MyCircuit.ToJSON(writer);
        writer.WriteLine("],");
    }
    public static Wand FromJSON(StreamReader reader)
    {
        string line = reader.ReadLine();
        if (line.Equals("null,"))
            return null;
        Shared.Assert(line.Equals("["));
        string wandType = JSON.ParseString(reader);
        Wand wand = null;
        switch (wandType)
        {
            case "test":
                wand = new Test();
                break;
            case "staff":
                wand = new Staff();
                break;
            default:
                throw new Shared.ValueError();
        }
        wand.MyCircuit = Circuit.FromJSON(reader, null);
        Shared.Assert(reader.ReadLine().Equals("],"));
        return wand;
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
            c.Add(new Gem.Mirror(false), new PointInt(3, 1));
            c.Add(new Gem.Mirror(true), new PointInt(4, 1));
            c.Add(new Gem.Stochastic(false), new PointInt(4, 4));
            c.Add(new Gem.Focus(new PointInt(1, 0)), new PointInt(3, 4));
            MyCircuit = c;
        }

        public override Wand UpgradeInto()
        {
            throw new NotImplementedException();
        }
    }
    public class Staff : Wand, SpawnableSpecial
    {
        public override string Name()
        {
            return "staff";
        }
        public override string DisplayName()
        {
            return "Weak Staff";
        }
        public override void Init()
        {
            Circuit c = new Circuit(new PointInt(4, 3));
            for (int i = 0; i < c.Size.IntX; i++)
            {
                c.Add(new Gem.Wall(), new PointInt(i, 0), false);
                c.Add(new Gem.Wall(), new PointInt(i, c.Size.IntY - 1), false);
            }

            Gem source = new Gem.Source(new PointInt(1, 0));
            c.Add(source, new PointInt(0, 1));

            Gem drain = new Gem.Drain();
            c.Add(drain, new PointInt(c.Size.IntX - 1, 1));

            MyCircuit = c;
        }

        public override Wand UpgradeInto()
        {
            throw new NotImplementedException();
        }
    }
}
