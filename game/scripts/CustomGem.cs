using System;
using System.Diagnostics;
using System.Collections.Generic;

public class CustomGem : Gem
{
    public Simplest MetaLevel;
    public Circuit MyCircuit;  // only one source one drain
    public Simplest CachedAdder;
    public Simplest CachedMultiplier;
    public CustomGem(Simplest metaLevel) : base()
    {
        MetaLevel = metaLevel;
        CachedAdder = null;
        CachedMultiplier = null;
        initCircuit();
    }
    private void initCircuit()
    {
        int n = 5;
        MyCircuit = new Circuit(new PointInt(n, n));
        for (int i = 0; i < n; i++)
        {
            MyCircuit.Add(new Gem.Wall().Place(new PointInt(0, i)), true);
            MyCircuit.Add(new Gem.Wall().Place(new PointInt(n - 1, i)), true);
            MyCircuit.Add(new Gem.Wall().Place(new PointInt(i, 0)), true);
            MyCircuit.Add(new Gem.Wall().Place(new PointInt(i, n - 1)), true);
        }

        PointInt location;
        location = new PointInt(n / 2, 0);
        MyCircuit.Remove(location);
        MyCircuit.Add(new Gem.Source(new PointInt(0, 1)).Place(location));

        location = new PointInt(n / 2, n - 1);
        MyCircuit.Remove(location);
        MyCircuit.Add(new Gem.Drain().Place(location));
    }
    public override string Name()
    {
        return "custom";
    }
    public override Particle Apply(Particle input)
    {
        Debug.Assert(CachedMultiplier != null);
        input.Multiply(CachedMultiplier);
        if (CachedAdder != null)
        {
            input.Mana[0] = Simplest.Eval(
                input.Mana[0], Operator.PLUS, CachedAdder
            );
        }
        return input;
    }
    public void Eval()
    {
        CachedAdder = null;
        if (MetaLevel.MyRank != Rank.FINITE)
        {
            // typeless custom gem
            // recursion makes the system of eq. non-linear, 
            // so I can't solve for finite solutions just by 
            // matrix division. Give up. Only get 
            // infinite solutions. 
            EvalTypeless eT = new EvalTypeless(this);
            CachedMultiplier = eT.Search();
            return;
        }
        // typed custom gem
        Simplest mse1 = MyCircuit.MinimumSuperpositionEquilibrium(1);
        if (mse1.MyRank == Rank.FINITE)
        {
            Simplest mse0 = MyCircuit.MinimumSuperpositionEquilibrium(0);
            CachedAdder = mse0;
            CachedMultiplier = new Simplest(Rank.FINITE, mse1.K - mse0.K);
        }
        else
        {
            CachedMultiplier = mse1;
        }
    }

    private class EvalTypeless : EstimateLFS
    {
        public CustomGem Parent;
        public EvalTypeless(CustomGem parent)
        {
            Parent = parent;
        }
        public override bool SolveRank(Rank rank, int k)
        {
            Simplest x = new Simplest(rank, k);
            Parent.CachedMultiplier = x;
            Source source = Parent.MyCircuit.FindAll<Source>()[0];
            List<Focus> focuses = Parent.MyCircuit.FindAll<Focus>();
            // List<CustomGem> typelesses;
            // {
            //     List<CustomGem> cGs = MyCircuit.FindAll<CustomGem>();
            //     cGs.RemoveAll(cG => cG.MetaLevel.MyRank == Rank.FINITE);
            //     typelesses = cGs;
            // }
            int n = focuses.Count;
            // int m = typelesses.Count;
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
            Simplest[] lhs = Simplest.Zeros(n);

            Queue<Particle> particles = new Queue<Particle>();
            Simplest[] mana;
            mana = Simplest.Ones(1);
            particles.Enqueue(new Particle(source.Location, null, mana));
            for (int i = 0; i < n; i++)
            {
                mana = new Simplest[1];
                mana[0] = x;
                Particle p = new Particle(focuses[i].Location, null, mana);
                p = focuses[i].Apply(p);
                p.Location += p.Direction;
                particles.Enqueue(p);
            }
            Simplest drainMana = null;

            while (particles.Count > 0)
            {
                Particle p = particles.Dequeue();
                Gem gem = Parent.MyCircuit.Seek(p.Location);
                if (gem is Drain drain)
                {
                    Console.WriteLine("drain got");
                    Console.WriteLine(p);
                    drainMana = p.Mana[0];
                }
                else if (gem is Focus focus)
                {
                    Console.WriteLine("focus got");
                    Console.WriteLine(p);
                    int iFocus = dictionary[focus];
                    lhs[iFocus] = Simplest.Eval(
                        lhs[iFocus], Operator.PLUS, p.Mana[0]
                    );
                }
                else
                {
                    foreach (Particle newP in Parent.MyCircuit.Advect(p, true, false))
                    {
                        particles.Enqueue(newP);
                    }
                }
            }

            // return true if solution >= needed
            if (drainMana == null)
                return true;
            if (drainMana <= x)
            {
                return true;
            }
            return false;
        }
    }

    public override string DisplayName()
    {
        if (MetaLevel.MyRank != Rank.FINITE)
            return "Typeless Custom Gem";
        int k = (int)MetaLevel.K;
        if (k == 0)
            return "Custom Gem";
        if (k == 1)
            return "Meta Custom Gem";
        if (k == 2)
            return "Meta Meta Custom Gem";
        return $"Meta^{k} Custom Gem";
    }
    public override string Explain()
    {
        throw new NotImplementedException();
    }
}
