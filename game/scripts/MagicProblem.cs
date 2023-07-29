using System;
using System.Linq;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MagicProblem
{
    private static readonly int UPRANK_EVERY = 17;
    public int N;
    public Circuit MyCircuit;
    public Gem.Source SourceGem;
    public PointInt SourceLocation;
    public List<(PointInt, Gem.Stochastic)> StochasticWithLocations;
    public List<(PointInt, Gem.Focus)> FocusWithLocations;
    Dictionary<Gem, int> Dictionary;
    public Simplest[,] AWithoutDiag;
    public Simplest[] MinusB;
    public Simplest[] Guess = null;

    public MagicProblem(Circuit circuit)
    {
        MyCircuit = circuit;
        (SourceLocation, SourceGem) = MyCircuit.FindAll<Gem.Source>()[0];
        StochasticWithLocations = MyCircuit.FindAll<Gem.Stochastic>();
        FocusWithLocations = MyCircuit.FindAll<Gem.Focus>();
        N = StochasticWithLocations.Count * 4 + FocusWithLocations.Count;
        Dictionary = new Dictionary<Gem, int>();
        {
            int i = 0;
            foreach (var (_, stochastic) in StochasticWithLocations)
            {
                Dictionary[stochastic] = i;
                i += 4;
            }
            foreach (var (_, focus) in FocusWithLocations)
            {
                Dictionary[focus] = i;
                i++;
            }
            Shared.Assert(i == N);
        }
    }

    public Simplest[] AdvectSuperposition(int inputMana)
    {
        AWithoutDiag = Simplest.Zeros(N, N);
        MinusB = Simplest.Zeros(N);
        Queue<Particle> particles = new Queue<Particle>();
        Simplest[] mana;
        mana = Simplest.Zeros(N + 1);
        mana[0].K = inputMana;
        particles.Enqueue(new Particle(
            SourceLocation + SourceGem.Direction, 
            SourceGem.Direction, mana
        ));
        for (int i = 0; i < N; i++)
        {
            mana = Simplest.Zeros(N + 1);
            if (Guess == null)
            {
                mana[i + 1].K = 1;
            }
            else
            {
                mana[0] = Guess[i];
            }
            Gem gem;
            Particle p = new Particle(null, null, mana);
            if (i < StochasticWithLocations.Count * 4)
            {
                (p.Location, gem) = StochasticWithLocations[i / 4];
                p.Direction = PointInt.PhaseToBaseVec(i % 4);
            }
            else
            {
                (p.Location, gem) = FocusWithLocations[
                    i - StochasticWithLocations.Count * 4
                ];
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
            Gem gem = MyCircuit.Seek(p.Location);
            switch (gem)
            {
                case Gem.Source source:
                    // remove particle
                    break;
                case Gem.Drain drain:
                    // Console.WriteLine("drain got " + p);
                    drainMana = (Simplest[])p.Mana.Clone();
                    break;
                case Gem.Focus _:
                case Gem.Stochastic _:
                    // Console.WriteLine(gem + " got " + p);
                    int i = Dictionary[gem];
                    if (gem is Gem.Focus)
                    {
                        JoinInto(i, p);
                    }
                    else if (gem is Gem.Stochastic stochastic)
                    {
                        if (Shared.DEBUG_MATH) {
                            Console.WriteLine("Hit stochastic.");
                            Console.WriteLine("in mana: " + p.Mana[0]);
                        }
                        p.Multiply(0.5);
                        int j = i + PointInt.BaseVecToPhase(p.Direction);
                        if (Shared.DEBUG_MATH) {
                            Console.WriteLine("halved mana: " + p.Mana[0]);
                            Console.WriteLine("j: " + j);
                        }
                        JoinInto(j, p);
                        p = stochastic.ApplyMirror(p);
                        j = i + PointInt.BaseVecToPhase(p.Direction);
                        if (Shared.DEBUG_MATH) {
                            Console.WriteLine("j: " + j);
                        }
                        JoinInto(j, p);
                    }
                    break;
                default:
                    foreach (Particle newP in MyCircuit.Advect(p, true, false))
                    {
                        particles.Enqueue(newP);
                        // Console.WriteLine("new particle from " + gem);
                    }
                    break;
            }
        }
        return drainMana;
    }

    public Vector<double> SolveFinite()
    {
        Simplest[] solution = new Simplest[N];
        Matrix<double> A = Matrix<double>.Build.Dense(N, N);
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Simplest s = AWithoutDiag[i, j];
                if (s.MyRank != Rank.FINITE)
                    return null;
                A[i, j] = s.K;
                if (i == j)
                    A[i, j]--;
            }
        }
        Vector<double> b = Vector<double>.Build.Dense(N);
        for (int i = 0; i < N; i++)
        {
            if (MinusB[i].MyRank != Rank.FINITE)
                return null;
            b[i] = -MinusB[i].K;
        }
        Vector<double> x = A.Solve(b);
        if (double.IsNaN(x.Minimum()) || x.Minimum() < -.00001)
        {
            if (Shared.DEBUG_MATH)
                Console.WriteLine("reject " + x);
            return null;    // reject NaN & negative solutions
        }
        return x;
    }

    public void Print()
    {
        Console.WriteLine("MagicProblem BEGIN");
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Console.Write(AWithoutDiag[i, j]);
                Console.Write(" ");
                Console.Write((char)((int)'A' + j));
                Console.Write(" + ");
            }
            Console.Write(MinusB[i]);
            Console.Write(" = ");
            Console.Write((char)((int)'A' + i));
            Console.WriteLine();
        }
        Console.WriteLine("MagicProblem END");
    }
    private void JoinInto(
        int receiverI, Particle p
    )
    {
        for (int j = 0; j < N; j++)
        {
            AWithoutDiag[receiverI, j] = Simplest.Eval(
                AWithoutDiag[receiverI, j],
                Operator.PLUS, p.Mana[j + 1]
            );
        }
        MinusB[receiverI] = Simplest.Eval(
            MinusB[receiverI],
            Operator.PLUS, p.Mana[0]
        );
    }

    public bool DoSatisfy()
    {
        for (int i = 0; i < N; i++)
        {
            if (!(MinusB[i] <= Simplest.Eval(
                Guess[i], Operator.PLUS, new Simplest(
                    Rank.FINITE, .1
                )
            )))
                return false;
        }
        return true;
    }

    public Simplest SolveTyped(int inputMana)
    {
        if (Shared.DEBUG_MATH)
            Console.WriteLine("SolveTyped begin");
        Simplest[] drainMana = AdvectSuperposition(inputMana);
        if (drainMana == null) {
            if (Shared.DEBUG_MATH)
                Console.WriteLine("drain collected no particles.");
            return Simplest.Zero();
        }
        Simplest acc = drainMana[0];
        if (N == 0) {
            if (Shared.DEBUG_MATH)
                Console.WriteLine("Problem size = 0.");
            return acc;
        }
        if (Shared.DEBUG_MATH)
            Print();
        Vector<double> x = SolveFinite();
        if (x != null)
        {
            for (int i = 0; i < N; i++)
            {
                acc = Simplest.Eval(
                    acc, Operator.PLUS,
                    Simplest.Eval(
                        new Simplest(Rank.FINITE, x[i]),
                        Operator.TIMES, drainMana[i + 1]
                    )
                );
            }
            if (Shared.DEBUG_MATH)
                Console.WriteLine("Finite solution found.");
            return acc;
        }
        if (Shared.DEBUG_MATH)
            Console.WriteLine("Finite solution not found.");
        Guess = Simplest.Zeros(N);
        int patience = UPRANK_EVERY;
        while (true)
        {
            bool accept = true;
            for (int i = 0; i < N; i++)
            {
                // if (!(Guess[i] <= MinusB[i]))
                // {
                //     Console.WriteLine(Guess[i] + " >" + MinusB[i]);
                //     throw new Shared.ValueError();
                // }
                if (!(MinusB[i] <= Simplest.Eval(
                    Guess[i], Operator.PLUS, new Simplest(
                        Rank.FINITE, .1
                    )
                )))
                {
                    accept = false;
                    if (Guess[i] <= MinusB[i])
                        Guess[i] = MinusB[i];
                    if (patience == 0)
                    {
                        patience = UPRANK_EVERY;
                        if (Guess[i].MyRank == Rank.STACK_W) {
                            // throw new Shared.PlayerCreatedEpsilonNaught();
                            Main.Singleton.MadeEpsilon0.PopupCentered();
                            return Simplest.Zero();
                        }
                        Guess[i] = Simplest.Bottom(Guess[i].MyRank + 1);
                    }
                }
            }
            if (accept) break;
            patience--;
            if (Shared.DEBUG_MATH) {
                Console.Write("Trying ");
                Shared.PrintArray(Guess);
            }
            drainMana = AdvectSuperposition(inputMana);
        }
        if (Shared.DEBUG_MATH) {
            Console.WriteLine("Guess:");
            foreach (Simplest s in Guess) {
                Console.Write(s);
                Console.Write(", ");
            }
            Console.WriteLine(". ");

            Console.WriteLine("MinusB:");
            foreach (Simplest s in MinusB) {
                Console.Write(s);
                Console.Write(", ");
            }
            Console.WriteLine(". ");
        }
        if (Shared.DEBUG_MATH) {
            Console.WriteLine("drainMana: " + drainMana[0]);
            Console.WriteLine("SolveTyped end");
        }
        return drainMana[0];
    }

    public Simplest SolveTypeless(CustomGem typelessGem)
    {
        Simplest tGuess = Simplest.Zero();
        int patience = UPRANK_EVERY;
        while (true)
        {
            typelessGem.CachedMultiplier = tGuess;
            typelessGem.CachedAdder = null;
            if (Shared.DEBUG_MATH)
                Console.WriteLine("Trying typeless = " + tGuess);
            Simplest drainMana = SolveTyped(1);
            if (Shared.DEBUG_MATH)
                Console.WriteLine("drainMana = " + drainMana);
            if (drainMana <= Simplest.Eval(
                tGuess, Operator.PLUS, new Simplest(
                    Rank.FINITE, .1
                )
            ))
            {
                return drainMana;
            }
            patience--;
            if (patience == 0)
            {
                if (tGuess.MyRank == Rank.STACK_W) {
                    // throw new Shared.PlayerCreatedEpsilonNaught();
                    Main.Singleton.MadeEpsilon0.PopupCentered();
                    return Simplest.Zero();
                }
                tGuess = Simplest.Bottom(tGuess.MyRank + 1);
            }
            else
            {
                tGuess = drainMana;
            }
        }
    }
}
