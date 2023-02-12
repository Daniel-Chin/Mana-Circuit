using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class MagicProblem
{
    public int N;
    public Simplest[,] AWithoutDiag;
    public Simplest[] MinusB;

    public MagicProblem(int n)
    {
        N = n;
        AWithoutDiag = Simplest.Zeros(N, N);
        MinusB = Simplest.Zeros(N);
    }

    public Simplest[] Solve()
    {
        Simplest[] solution = new Simplest[N];
        Vector<double> x = SolveFinite();
        if (x != null)
        {
            for (int i = 0; i < N; i++)
            {
                solution[i] = new Simplest(Rank.FINITE, x[i]);
            }
            return solution;
        }
        Console.WriteLine("No finite solution.");
        Simplest simplest = SolveInfinite();
        for (int i = 0; i < N; i++)
        {
            solution[i] = simplest;
        }
        return solution;
    }

    private Vector<double> SolveFinite()
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
            b[i] = -MinusB[i].K;
        }
        Vector<double> x = A.Solve(b);
        if (double.IsNaN(x.Minimum()) || x.Minimum() < 0)
            return null;    // reject NaN & negative solutions
        return x;
    }

    private Simplest SolveInfinite()
    {
        if (SolveRank(Rank.TWO_TO_THE_W, -1))
        {
            // Console.WriteLine("TWO_TO_THE_W is a solution.");
            ExponentialSearch eSearch = new ExponentialSearch(2333);
            try
            {
                while (true)
                {
                    if (eSearch.Feedback(SolveRank(Rank.W_TO_THE_K, eSearch.Acc)))
                        break;
                }
            }
            catch (ExponentialSearch.SearchFailed)
            {
                return new Simplest(Rank.TWO_TO_THE_W, -1);
            }
            return new Simplest(Rank.W_TO_THE_K, eSearch.High);
        }
        else
        {
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
        Simplest x = new Simplest(rank, k);
        Console.Write("Trying ");
        Console.WriteLine(x);
        bool doAccept = true;
        for (int i = 0; i < N; i++)
        {
            Simplest acc = Simplest.Zero();
            for (int j = 0; j < N; j++)
            {
                acc = Simplest.Eval(
                    acc, Operator.PLUS, Simplest.Eval(
                        x, Operator.TIMES, AWithoutDiag[i, j]
                    )
                );
            }
            acc = Simplest.Eval(
                acc, Operator.PLUS, MinusB[i]
            );
            if (!acc.Equals(x))
            {
                doAccept = false;
                break;
            }
        }
        Console.Write("Accept? ");
        Console.WriteLine(doAccept);
        return doAccept;
    }

    public static void Test()
    {
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
}
