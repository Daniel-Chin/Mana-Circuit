using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MagicProblem {
    public List<List<Simplest>> LeftHandSide;

    public Simplest[] Solve() {
        foreach (Rank rank in Enum.GetValues(typeof(Rank)))
        {
            Simplest[] solution = SolveAssuming(rank);
            if (solution == null)
            {}
        }
    }

    private Simplest[] SolveAssuming(Rank rank) {
        int n = LeftHandSide.Count;
        Simplest[] solution = new Simplest[n];
        switch (rank) {
            case Rank.FINITE:
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
                for (int i = 0; i < n; i++)
                {
                    solution[i] = new Simplest(Rank.FINITE, x[i]);
                }
                return solution;
            case Rank.W_TO_THE_K:
                break;
            case Rank.TWO_TO_THE_W:
                break;
            case Rank.STACK_W:
                break;
        }
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
