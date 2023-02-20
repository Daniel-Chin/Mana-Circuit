using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Circuit
{
    public PointInt Size;
    public Gem[,] Field;
    public List<Gem> Gems;

    class Collision : Exception { }

    public Circuit(PointInt size)
    {
        Size = size;
        Field = new Gem[size.IntX, size.IntY];
        Gems = new List<Gem>();
    }

    public void Add(Gem gem, PointInt location)
    {
        Debug.Assert(
            gem.Locations.Count < 1
            || gem is CustomGem cG
        );
        if (
            location.IntX < 0 ||
            location.IntY < 0 ||
            location.IntX + gem.Size.IntX > Size.IntX ||
            location.IntY + gem.Size.IntY > Size.IntY
        )
            throw new Collision();
        if (IterRect(false, null, location, gem.Size))
        {
            if (!Gems.Contains(gem))
                Gems.Add(gem);
            IterRect(true, gem, location, gem.Size);
            gem.Locations.Add(location);
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
        gem.Locations.Remove(location);
        if (gem.Locations.Count == 0)
            Gems.Remove(gem);
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

    public List<T> FindAll<T>()
    {
        List<T> matches = new List<T>();
        foreach (Gem gem in Gems)
        {
            if (gem is T t)
            {
                matches.Add(t);
            }
        }
        return matches;
    }

    public Gem Seek(PointInt location)
    {
        return Field[location.IntX, location.IntY];
    }
}
