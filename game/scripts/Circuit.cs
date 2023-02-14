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

    public void Add(Gem gem)
    {
        if (
            gem.Location.IntX < 0 ||
            gem.Location.IntY < 0 ||
            gem.Location.IntX + gem.Size.IntX > Size.IntX ||
            gem.Location.IntY + gem.Size.IntY > Size.IntY
        )
            throw new Collision();
        if (IterRect(false, null, gem.Location, gem.Size))
        {
            Gems.Add(gem);
            IterRect(true, gem, gem.Location, gem.Size);
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

    public void Remove(Gem gem)
    {
        Gems.Remove(gem);
        IterRect(true, null, gem.Location, gem.Size);
        Gem g = Seek(gem.Location);
    }
    public void Remove(PointInt location)
    {
        Gem gem = Seek(location);
        if (gem != null)
            Remove(gem);
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

    public Simplest MinimumSuperpositionEquilibrium(int inputMana)
    {
        Gem.Source source = FindAll<Gem.Source>()[0];
        List<Gem.Focus> focuses = FindAll<Gem.Focus>();
        int n = focuses.Count;
        MagicProblem magicProblem = new MagicProblem(n);
        Dictionary<Gem.Focus, int> dictionary = new Dictionary<Gem.Focus, int>();
        {
            int i = 0;
            foreach (Gem.Focus focus in focuses)
            {
                dictionary[focus] = i;
                i++;
            }
            Debug.Assert(i == n);
        }

        Queue<Particle> particles = new Queue<Particle>();
        Simplest[] mana;
        mana = Simplest.Zeros(n + 1);
        mana[0].K = inputMana;
        particles.Enqueue(new Particle(source.Location, null, mana));
        for (int i = 0; i < n; i++)
        {
            mana = Simplest.Zeros(n + 1);
            mana[i + 1].K = 1;
            Particle p = new Particle(focuses[i].Location, null, mana);
            p = focuses[i].Apply(p);
            p.Location += p.Direction;
            particles.Enqueue(p);
        }
        Simplest[] drainMana = null;

        while (particles.Count > 0)
        {
            // Console.Write("particles.Count ");
            // Console.WriteLine(particles.Count);
            Particle p = particles.Dequeue();
            Gem gem = Seek(p.Location);
            if (gem is Gem.Drain drain)
            {
                Console.WriteLine("drain got");
                Console.WriteLine(p);
                drainMana = (Simplest[])p.Mana.Clone();
            }
            else if (gem is Gem.Focus focus)
            {
                Console.WriteLine("focus got");
                Console.WriteLine(p);
                int iFocus = dictionary[focus];
                for (int j = 0; j < n; j++)
                {
                    magicProblem.AWithoutDiag[iFocus, j] = Simplest.Eval(
                        magicProblem.AWithoutDiag[iFocus, j],
                        Operator.PLUS, p.Mana[j + 1]
                    );
                }
                magicProblem.MinusB[iFocus] = Simplest.Eval(
                    magicProblem.MinusB[iFocus],
                    Operator.PLUS, p.Mana[0]
                );
            }
            else
            {
                foreach (Particle newP in Advect(p, true, false))
                {
                    particles.Enqueue(newP);
                }
            }
        }

        if (drainMana == null)
            return Simplest.Zero();
        magicProblem.Print();
        Simplest acc = drainMana[0];
        if (n != 0)
        {
            Simplest[] solution = magicProblem.Solve();
            for (int i = 0; i < n; i++)
            {
                acc = Simplest.Eval(
                    acc, Operator.PLUS,
                    Simplest.Eval(solution[i], Operator.TIMES, drainMana[i + 1])
                );
            }
        }

        return acc;
    }
}
