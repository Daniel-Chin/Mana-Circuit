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
        // if empty already, silent.
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

    public (MagicProblem, Simplest[]) StaticAnalysis(
        Simplest guess, // =null to denote each static source as an unknown
        int inputMana
    )
    {
        Gem.Source source = FindAll<Gem.Source>()[0];
        List<Gem.Stochastic> stochastics = FindAll<Gem.Stochastic>();
        List<Gem.Focus> focuses = FindAll<Gem.Focus>();
        int n = stochastics.Count * 4 + focuses.Count;
        MagicProblem magicProblem = new MagicProblem(n);
        Dictionary<Gem, int> dictionary = new Dictionary<Gem, int>();
        {
            int i = 0;
            foreach (Gem stochastic in stochastics)
            {
                dictionary[stochastic] = i;
                i += 4;
            }
            foreach (Gem focus in focuses)
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
            if (guess == null)
            {
                mana[i + 1].K = 1;
            }
            else
            {
                mana[0] = guess;
            }
            Gem gem;
            Particle p = new Particle(null, null, mana);
            if (i < stochastics.Count * 4)
            {
                gem = stochastics[i / 4];
                p.Location = gem.Location;
                p.Direction = PointInt.PhaseToBaseVec(i % 4);
            }
            else
            {
                gem = focuses[i - stochastics.Count * 4];
                p.Location = gem.Location;
                p = gem.Apply(p);
            }
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
            switch (gem)
            {
                case Gem.Drain drain:
                    // Console.WriteLine("drain got " + p);
                    drainMana = (Simplest[])p.Mana.Clone();
                    break;
                case Gem.Focus _:
                case Gem.Stochastic _:
                    // Console.WriteLine(gem + " got " + p);
                    int i = dictionary[gem];
                    if (gem is Gem.Focus)
                    {
                        JoinInto(magicProblem, i, p);
                    }
                    else if (gem is Gem.Stochastic stochastic)
                    {
                        p.Multiply(0.5);
                        int j = i + PointInt.BaseVecToPhase(p.Direction);
                        JoinInto(magicProblem, j, p);
                        p = stochastic.ApplyMirror(p);
                        j = i + PointInt.BaseVecToPhase(p.Direction);
                        JoinInto(magicProblem, j, p);
                    }
                    break;
                default:
                    foreach (Particle newP in Advect(p, true, false))
                    {
                        particles.Enqueue(newP);
                        // Console.WriteLine("new particle from " + gem);
                    }
                    break;
            }
        }
        return (magicProblem, drainMana);
    }

    private void JoinInto(
        MagicProblem magicProblem, int receiverI, Particle p
    )
    {
        for (int j = 0; j < magicProblem.N; j++)
        {
            magicProblem.AWithoutDiag[receiverI, j] = Simplest.Eval(
                magicProblem.AWithoutDiag[receiverI, j],
                Operator.PLUS, p.Mana[j + 1]
            );
        }
        magicProblem.MinusB[receiverI] = Simplest.Eval(
            magicProblem.MinusB[receiverI],
            Operator.PLUS, p.Mana[0]
        );
    }

    public bool DoSatisfy(
        Simplest guess, MagicProblem magicProblem
    )
    {
        foreach (Simplest s in magicProblem.MinusB)
        {
            if (!(s <= guess))
                return false;
        }
        return true;
    }
}
