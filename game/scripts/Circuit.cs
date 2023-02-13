using System;
using System.Collections.Generic;

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

    public void Add(Gem gem)
    {
        if (
            gem.Location.IntX < 0 ||
            gem.Location.IntY < 0 ||
            gem.Location.IntX + gem.Size.IntX > Size.IntX ||
            gem.Location.IntY + gem.Size.IntY > Size.IntY
        )
            throw new Collision();
        if (IterRect(null, gem.Location, gem.Size))
        {
            Gems.Add(gem);
            IterRect(gem, gem.Location, gem.Size);
        }
        else
        {
            throw new Collision();
        }
    }
    public bool Add(Gem gem, bool swallowCollision)
    {
        if (swallowCollision)
        {
            try
            {
                Add(gem);
            }
            catch (Collision)
            {
                return false;
            }
        }
        else
        {
            Add(gem);
        }
        return true;
    }

    private bool IterRect(
        Gem action, PointInt location, PointInt size
    )
    {
        for (int dx = 0; dx < size.IntX; dx++)
        {
            for (int dy = 0; dy < size.IntY; dy++)
            {
                int x = location.IntX + dx;
                int y = location.IntY + dy;
                if (action == null)
                {
                    // check
                    if (Field[x, y] != null)
                        return false;
                }
                else
                {
                    // fill
                    Field[x, y] = action;
                }
            }
        }
        return true;
    }

    public void Remove(Gem gem)
    {
        Gems.Remove(gem);
        IterRect(null, gem.Location, gem.Size);
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
            // wall
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
