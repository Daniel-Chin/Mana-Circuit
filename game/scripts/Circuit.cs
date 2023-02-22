using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class Circuit : JSONable
{
    public PointInt Size { get; set; }
    public Gem[,] Field { get; set; }

    class Collision : Exception { }

    public Circuit(PointInt size)
    {
        Size = size;
        Field = new Gem[size.IntX, size.IntY];
    }

    public void Add(Gem gem, PointInt location)
    {
        if (
            location.IntX < 0 ||
            location.IntY < 0 ||
            location.IntX + gem.Size.IntX > Size.IntX ||
            location.IntY + gem.Size.IntY > Size.IntY
        )
            throw new Collision();
        if (IterRect(false, null, location, gem.Size))
        {
            IterRect(true, gem, location, gem.Size);
        }
        else
        {
            throw new Collision();
        }
    }
    public bool Add(Gem gem, PointInt location, bool swallowCollision)
    {
        if (swallowCollision)
        {
            try
            {
                Add(gem, location);
            }
            catch (Collision)
            {
                return false;
            }
        }
        else
        {
            Add(gem, location);
        }
        return true;
    }

    private bool IterRect(
        bool isPlaceNotCheck,
        Gem toPlace, PointInt location, PointInt size
    )
    {
        for (int dx = 0; dx < size.IntX; dx++)
        {
            for (int dy = 0; dy < size.IntY; dy++)
            {
                int x = location.IntX + dx;
                int y = location.IntY + dy;
                if (isPlaceNotCheck)
                {
                    Field[x, y] = toPlace;
                }
                else
                {
                    if (Field[x, y] != null)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void Remove(PointInt location)
    {
        Gem gem = Seek(location);
        // if empty already, silent.
        if (gem == null) return;
        IterRect(true, null, location, gem.Size);
    }

    public Particle[] Advect(
        Particle particle, bool superposition, bool verbose
    )
    {
        Gem gem = Seek(particle.Location);
        Particle[] results;
        if (gem == null || gem == particle.LastGem)
        {
            results = new Particle[1];
            results[0] = particle;
        }
        else
        {
            if (verbose)
            {
                Console.Write("Enter ");
                Console.WriteLine(gem);
            }
            if (superposition && gem is Gem.Stochastic stochastic)
            {
                results = stochastic.Superposition(particle);
            }
            else
            {
                results = new Particle[1];
                results[0] = gem.Apply(particle);
            }
        }
        if (results[0] == null)
        {
            // wall or drain
            return new Particle[0];
        }
        foreach (Particle p in results)
        {
            p.Location += p.Direction;
            p.LastGem = gem;
        }
        return results;
    }

    public List<(PointInt, T)> FindAll<T>()
    {
        List<(PointInt, T)> matches = new List<(PointInt, T)>();
        for (int i = 0; i < Size.IntX; i++)
        {
            for (int j = 0; j < Size.IntY; j++)
            {
                Gem gem = Field[i, j];
                if (gem is T t)
                {
                    matches.Add((new PointInt(i, j), t));
                }
            }
        }
        return matches;
    }

    public Gem Seek(PointInt location)
    {
        return Field[location.IntX, location.IntY];
    }

    public void ToJSON(StreamWriter writer)
    {
        writer.WriteLine("[");
        Size.ToJSON(writer);
        for (int i = 0; i < Size.IntX; i++)
        {
            for (int j = 0; j < Size.IntY; j++)
            {
                Gem gem = Field[i, j];
                if (gem == null)
                {
                    writer.WriteLine("null,");
                }
                else
                {
                    gem.ToJSON(writer, true);
                }
            }
        }
        writer.WriteLine("],");
    }
    public static Circuit FromJSON(StreamReader reader, CustomGem typeless)
    {
        Debug.Assert(reader.ReadLine().Equals("["));
        PointInt size = PointInt.FromJSON(reader);
        Circuit c = new Circuit(size);
        for (int i = 0; i < size.IntX; i++)
        {
            for (int j = 0; j < size.IntY; j++)
            {
                PointInt location = new PointInt(i, j);
                Gem gem = Gem.FromJSON(reader, true);
                if (gem == null)
                    continue;
                if (
                    typeless != null && gem is CustomGem cG
                    && cG.MetaLevel.MyRank != Rank.FINITE
                )
                {
                    gem = typeless;
                }
                c.Add(gem, location);
            }
        }
        Debug.Assert(reader.ReadLine().Equals("],"));
        return c;
    }
}
