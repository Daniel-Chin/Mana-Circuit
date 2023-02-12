using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class Circuit
{
    public Vector<int> Size;
    public Gem[,] Matrix;
    public List<Gem> Gems;

    class Collision : Exception { }

    public Circuit(Vector<int> size)
    {
        Size = size;
        Matrix = new Gem[size[0], size[1]];
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
                if (action == null) {
                    // check
                    if (Matrix[x, y] != null)
                        return false;
                } else {
                    // fill
                    Matrix[x, y] = action;
                }
            }
        }
        return true;
    }

    public void Remove(Gem gem) {
        Gems.Remove(gem);
        IterRect(null, gem.Location, gem.Size);
    }

    public Particle Advect(Particle particle, Gem lastGem) {

    }

    public List<Gem.Source> FindSources() {
        List<Gem.Source> sources = new List<Gem.Source>();
        foreach (Gem gem in Gems)
        {
            if (gem is Gem.Source source) {
                sources.Add(source);
            }
        }
        return sources;
    }
}
