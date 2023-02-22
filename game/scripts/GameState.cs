using System;
using System.IO;

using System.Collections.Generic;
using Godot;

public class GameState
{
    public static PersistentClass Persistent;
    public static TransientClass Transient;
    static GameState()
    {
        Persistent = new PersistentClass();
        Transient = new TransientClass();
    }

    public class PersistentClass : JSONable
    {
        public Simplest Money { get; set; }
        public double Location_theta { get; set; }
        public Simplest Location_dist { get; set; }
        public Dictionary<string, int> HasGems { get; set; }
        public Dictionary<int, (int, CustomGem)> HasCustomGems { get; set; }
        public CustomGem MyTypelessGem { get; set; }
        public Wand MyWand { get; set; }

        // events. true means completed.
        public bool Event_Intro { get; set; }
        public int Event_JumperStage { get; set; }

        public int Loneliness_Shop { get; set; }
        public int Loneliness_GemExpert { get; set; }
        public int Loneliness_WandSmith { get; set; }
        public PersistentClass()
        {
            Money = Simplest.Zero();
            Location_theta = 0;
            Location_dist = Simplest.Zero();
            MyWand = null;
            HasGems = new Dictionary<string, int>();
            HasCustomGems = new Dictionary<int, (int, CustomGem)>();
            MyTypelessGem = null;

            Event_Intro = false;
            Event_JumperStage = 0;
            Loneliness_Shop = 0;
            Loneliness_GemExpert = 0;
            Loneliness_WandSmith = 0;

            DebugInit();
        }

        public void DebugInit()
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

            Ready();
        }
        public void ToJSON(StreamWriter writer)
        {
            writer.WriteLine("[");
            Money.ToJSON(writer);
            writer.Write(Location_theta);
            writer.WriteLine(',');
            Location_dist.ToJSON(writer);

            writer.WriteLine("[");
            foreach (var entry in HasGems)
            {
                JSON.Store(entry.Key, writer);
                writer.Write(entry.Value);
                writer.WriteLine(',');
            }
            writer.WriteLine("],");

            writer.WriteLine("[");
            foreach (var entry in HasCustomGems)
            {
                writer.Write(entry.Key);
                writer.WriteLine(',');
                writer.Write(entry.Value.Item1);
                writer.WriteLine(',');
                entry.Value.Item2.ToJSON(writer, false);
            }
            writer.WriteLine("],");

            if (MyTypelessGem == null)
            {
                writer.WriteLine("null,");
            }
            else
            {
                MyTypelessGem.ToJSON(writer, false);
            }
            if (MyWand == null)
            {
                writer.WriteLine("null,");
            }
            else
            {
                MyWand.ToJSON(writer);
            }

            JSON.Store(Event_Intro, writer);
            writer.Write(Event_JumperStage);
            writer.WriteLine(',');
            writer.Write(Loneliness_Shop);
            writer.WriteLine(',');
            writer.Write(Loneliness_GemExpert);
            writer.WriteLine(',');
            writer.Write(Loneliness_WandSmith);
            writer.WriteLine(',');

            writer.WriteLine("],");
        }
        public void FromJSON(StreamReader reader)
        {
            Shared.Assert(reader.ReadLine().Equals("["));

            Money = Simplest.FromJSON(reader);
            Location_theta = Double.Parse(JSON.NoLast(reader));
            Location_dist = Simplest.FromJSON(reader);

            Shared.Assert(reader.ReadLine().Equals("["));
            while (!JSON.DidArrayEnd(reader))
            {
                string key = JSON.ParseString(reader);
                int value = Int32.Parse(JSON.NoLast(reader));
                HasGems[key] = value;
            }

            Shared.Assert(reader.ReadLine().Equals("["));
            while (!JSON.DidArrayEnd(reader))
            {
                int key = Int32.Parse(JSON.NoLast(reader));
                int n_owned = Int32.Parse(JSON.NoLast(reader));
                CustomGem cG = (CustomGem)Gem.FromJSON(reader, false);
                HasCustomGems[key] = (n_owned, cG);
            }

            MyTypelessGem = (CustomGem)Gem.FromJSON(reader, false);
            MyWand = Wand.FromJSON(reader);

            Event_Intro = JSON.ParseBool(reader);
            Event_JumperStage = Int32.Parse(JSON.NoLast(reader));
            Loneliness_Shop = Int32.Parse(JSON.NoLast(reader));
            Loneliness_GemExpert = Int32.Parse(JSON.NoLast(reader));
            Loneliness_WandSmith = Int32.Parse(JSON.NoLast(reader));

            Shared.Assert(reader.ReadLine().Equals("],"));

            Persistent.Ready();
        }
        public void Ready()
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

    public class TransientClass
    {
        public bool NPCPausedWorld;
        public Vector2 LocationOffset;
        public int EventProgression;
        public Spawnable NextSpawn;
        public int EnemiesTillNextSpawn;
        public TransientClass()
        {
            NPCPausedWorld = false;
            LocationOffset = new Vector2(0, 0);
            EventProgression = 0;
            NextSpawn = null;
            EnemiesTillNextSpawn = 0;
        }
    }
}
