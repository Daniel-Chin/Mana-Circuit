using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MagicProblem {
    public List<List<Simplest>> LeftHandSide;

    public Simplest[] Solve() {
        int n = LeftHandSide.Count;
        Simplest[] solution = new Simplest[n];
        Vector<double> x = SolveFinite();
        if (x != null) {
            // todo
            return solution;
        }
        Simplest simplest = SolveInfinite();
    }

    private Vector<double> SolveFinite() {
        int n = LeftHandSide.Count;
        Simplest[] solution = new Simplest[n];
        Matrix<double> A = Matrix<double>.Build.Dense(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Simplest s = LeftHandSide[i][j];
                if (s.MyRank != Rank.FINITE)
                    return null;
                A[i, j] = s.K;
                if (i == j)
                    A[i, j] --;
            }
        }
        Vector<double> b = Vector<double>.Build.Dense(n);
        Vector<double> x = A.Solve(b);
        if (double.IsNaN(x.Minimum()) || x.Minimum() < 0)
            return null;    // reject NaN & negative solutions
        return x;
    }

    private Simplest SolveInfinite()
    {
        if (SolveRank(Rank.TWO_TO_THE_W, -1)) {
            ExponentialSearch eSearch = new ExponentialSearch(2333);
            try
            {
                while (! eSearch.Feedback(SolveRank(Rank.W_TO_THE_K, eSearch.Acc)))
                { }
            }
            catch (ExponentialSearch.SearchFailed)
            {
                return new Simplest(Rank.TWO_TO_THE_W, -1);
            }
            return new Simplest(Rank.W_TO_THE_K, eSearch.Acc + 1);
        } else {
            for (int k = 0; k < 69; k++)
            {
                if (SolveRank(Rank.STACK_W, k))
                    return new Simplest(Rank.STACK_W, k);
            }
            throw new Shared.PlayerCreatedEpsilonNaught();
            // and I don't know how they did it
        }
    }
    private bool SolveRank(Rank rank, int k)
    {

    }

    public static void Test() {
        // Matrix<double> A = DenseMatrix.OfArray(new double[,] {
        //         {1,1,1,1},
        //         {1,2,3,4},
        //         {4,3,2,1}});
        // Vector<double>[] nullspace = A.Kernel();

        // // verify: the following should be approximately (0,0,0)
        // var x = (A * (2*nullspace[0] - 3*nullspace[1]));
        // Console.Write("x = ");
        // Console.WriteLine(x);

        Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            {1,1,1},
            {1,2,3},
            {4,3,2}
        });
        Vector<double> b = DenseVector.OfArray(new double[] {
            0, 0, 0
        });
        var x = A.Solve(b);
        Console.WriteLine(x);
        x[1] = 3;
        Console.WriteLine(x);
        Console.WriteLine(x.Minimum());
        Console.ReadKey();
    }
}
