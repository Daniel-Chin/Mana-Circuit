using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

public abstract class EstimateLFS
{   // Least F-closed superposition
    private Simplest _solution = null;
    public abstract bool Try(Simplest guess);

    public Simplest Search()
    {
        _solution = null;
        for (int k = 1; k < 69; k++)
        {
            Helper(new Simplest(Rank.W_TO_THE_K, k));
            if (!Shared.DEBUG && _solution != null)
                return _solution;
        }
        Helper(new Simplest(Rank.TWO_TO_THE_W, -1));
        if (!Shared.DEBUG && _solution != null)
            return _solution;
        for (int k = 2; k < 69; k++)
        {
            Helper(new Simplest(Rank.STACK_W, k));
            if (!Shared.DEBUG && _solution != null)
                return _solution;
        }
        if (_solution == null)
        {
            throw new Shared.PlayerCreatedEpsilonNaught();
            // and I don't know how they did it
        }
        return _solution;
    }

    private void Helper(Simplest guess)
    {
        if (Try(guess))
        {
            if (_solution == null)
                _solution = guess;
        }
        else
        {
            if (_solution != null)
            {
                Console.WriteLine("Satisfiability not monotonous!?");
                Console.WriteLine("Previous solution: " + _solution);
                Console.WriteLine("Not a solution: " + guess);
                throw new Shared.ObjectStateIllegal();
            }
        }
    }

    public class Typed : EstimateLFS
    {
        private Circuit _circuit;
        private int _inputMana;
        public Typed(Circuit circuit, int inputMana)
        {
            _circuit = circuit;
            _inputMana = inputMana;
        }
        public Simplest Solve()
        {
            var (
                magicProblem, drainMana
            ) = _circuit.StaticAnalysis(null, _inputMana);
            if (drainMana == null)
                return Simplest.Zero();
            Simplest acc = drainMana[0];
            if (magicProblem.N == 0)
                return acc;
            magicProblem.Print();
            Vector<double> x = magicProblem.SolveFinite();
            if (x != null)
            {
                for (int i = 0; i < magicProblem.N; i++)
                {
                    acc = Simplest.Eval(
                        acc, Operator.PLUS,
                        Simplest.Eval(
                            new Simplest(Rank.FINITE, x[i]),
                            Operator.TIMES, drainMana[i + 1]
                        )
                    );
                }
                return acc;
            }
            Console.WriteLine("Finite solution doesn't exist.");
            return Search();
        }

        public override bool Try(Simplest guess)
        {
            var (
                magicProblem, drainMana
            ) = _circuit.StaticAnalysis(guess, _inputMana);
            return _circuit.DoSatisfy(guess, magicProblem);
        }
    }

    public class Typeless
    {
        private Typed _typed;
        private CustomGem _typelessGem;
        public Typeless(CustomGem typelessGem)
        {
            _typelessGem = typelessGem;
            _typed = new Typed(_typelessGem.MyCircuit, 1);
        }
        public (bool, Simplest) TryRank(Rank rank)
        {
            Simplest guess = Simplest.Bottom(rank);
            Simplest result = guess;
            for (int i = 0; i < 69; i++)    // iter
            {
                guess = result;
                _typelessGem.CachedAdder = null;
                _typelessGem.CachedMultiplier = guess;
                result = _typed.Solve();
                if (guess.Equals(result))
                    return (true, guess);
            }
            Simplest delta = Simplest.Eval(
                result, Operator.MINUS, guess
            );
            if (delta <= new Simplest(Rank.FINITE, 0.1))
                return (true, guess);
            return (false, guess);
        }

        public Simplest Solve()
        {
            Rank tryingRank = Rank.FINITE;
            while (tryingRank <= Rank.STACK_W)
            {
                var (accepted, solution) = TryRank(tryingRank);
                if (accepted) return solution;
                tryingRank = solution.MyRank + 1;
            }

            throw new Shared.PlayerCreatedEpsilonNaught();
            // and I don't know how they did it
        }
    }
}
