using System;
using System.Diagnostics;
using System.Collections.Generic;

class CustomGem : Gem
{
    public Simplest MetaLevel;
    public Circuit MyCircuit;  // only one source one drain
    public Simplest CachedMultiplier;
    public override Particle Apply(Particle input)
    {
        Debug.Assert(CachedMultiplier != null);
        input.Multiply(CachedMultiplier);
        return input;
    }
    public Simplest MinimumSuperpositionEquilibrium(int inputMana)
    {
        Source source = MyCircuit.FindAll<Source>()[0];
        List<Focus> focuses = MyCircuit.FindAll<Focus>();
        int n = focuses.Count;
        MagicProblem magicProblem = new MagicProblem(n);
        Dictionary<Focus, int> dictionary = new Dictionary<Focus, int>();
        {
            int i = 0;
            foreach (Focus focus in focuses)
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
            Gem gem = MyCircuit.Seek(p.Location);
            if (gem is Drain drain)
            {
                drainMana = p.Mana;
                // Console.WriteLine("drain hit");
            }
            else if (gem is Focus focus)
            {
                // Console.WriteLine("focus hit");
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
                foreach (Particle newP in MyCircuit.Advect(p, true, false))
                {
                    particles.Enqueue(newP);
                }
            }
        }

        magicProblem.Print();
        Simplest[] solution = magicProblem.Solve();
        Simplest acc = drainMana[0];
        for (int i = 0; i < n; i++)
        {
            acc = Simplest.Eval(
                acc, Operator.PLUS,
                Simplest.Eval(solution[i], Operator.TIMES, drainMana[i + 1])
            );
        }

        return acc;
    }
}
