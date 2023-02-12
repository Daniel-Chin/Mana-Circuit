using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class Circuit
{
    public Vector<int> Size;
    public Gem[,] Field;
    public List<Gem> Gems;

    class Collision : Exception { }

    public Circuit(Vector<int> size)
    {
        Size = size;
        Field = new Gem[size[0], size[1]];
        Gems = new List<Gem>();
    }

    public void Add(Gem gem)
    {
        if (
            gem.Location[0] < 0 ||
            gem.Location[1] < 0 ||
            gem.Location[0] + gem.Size[0] >= Size[0] ||
            gem.Location[1] + gem.Size[1] >= Size[1]
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

    private bool IterRect(
        Gem action, Vector<int> location, Vector<int> size
    )
    {
        for (int dx = 0; dx < size[0]; dx++)
        {
            for (int dy = 0; dy < size[1]; dy++)
            {
                int x = location[0] + dx;
                int y = location[1] + dy;
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
        Particle particle, bool superposition
    )
    {
        Gem gem = Seek(particle.Location);
        Particle[] results;
        if (gem == particle.LastGem)
        {
            results = new Particle[1];
            results[0] = particle;
        }
        else
        {
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

    public Gem Seek(Vector<int> location)
    {
        return Field[location[0], location[1]];
    }
}
